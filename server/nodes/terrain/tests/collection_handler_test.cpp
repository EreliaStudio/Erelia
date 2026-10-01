#include "erelia/server/router.hpp"
#include "terrain_node.hpp"
#include <chrono>
#include <gtest/gtest.h>
#include <network/client.hpp>
#include <thread>
namespace
{
	class TerrainHandlers : public testing::Test
	{
	protected:
		TerrainNode terrain{{.port = 0}};
		std::unique_ptr<Router> router;
		spk::Client client;
		std::vector<spk::Message> received, batch;
		void SetUp() override
		{
			terrain.start();
			router = std::make_unique<Router>(Router::Configuration{.port = 0, .nodeReconnectDelay = std::chrono::milliseconds(1), .nodes = {{.name = "terrain", .address = "127.0.0.1", .port = terrain.port()}}});
			router->redirect(static_cast<spk::Message::Type>(Networking::MessageType::ChunkRequest), "terrain");
			router->redirect(static_cast<spk::Message::Type>(Networking::MessageType::ColumnRequest), "terrain");
			router->start();
			client.connect("127.0.0.1", router->port());
		}
		void TearDown() override
		{
			client.disconnect();
			router->stop();
			terrain.stop();
		}
		void pump()
		{
			router->dispatch();
			terrain.dispatch();
			router->dispatch();
			for (const auto &message : client.messages().drain(batch))
			{
				received.push_back(message);
			}
		}
		bool wait(std::size_t count)
		{
			const auto deadline = std::chrono::steady_clock::now() + std::chrono::seconds(2);
			while (received.size() < count && std::chrono::steady_clock::now() < deadline)
			{
				pump();
				std::this_thread::yield();
			}
			return received.size() >= count;
		}
	};
}
TEST_F(TerrainHandlers, BothFamiliesEachProduceExactlyOneResponse)
{
	client.send(Networking::ChunkProtocol::Request::build(1, {{1, 0, 1}}));
	client.send(Networking::ColumnProtocol::Request::build(1, {{4, 4}}));
	ASSERT_TRUE(wait(2));
	for (int index = 0; index < 10; ++index)
	{
		pump();
	}
	ASSERT_EQ(received.size(), 2u);
	for (const auto &message : received)
	{
		EXPECT_EQ(message.requestID(), 1u);
		if (message.type() == static_cast<spk::Message::Type>(Networking::MessageType::ChunkResponse))
		{
			EXPECT_EQ(Networking::ChunkProtocol::Response(message).section(0).success.front().element.at({4, 1, 4}).definitionId(), 2u);
		}
		else
		{
			EXPECT_EQ(Networking::ColumnProtocol::Response(message).section(0).success.front().element.chunks.size(), 4u);
		}
	}
}
TEST_F(TerrainHandlers, MalformedRequestsEmitFamilyErrorsWithoutAcquisition)
{
	for (auto type : {Networking::MessageType::ChunkRequest, Networking::MessageType::ColumnRequest})
	{
		spk::Message::Writer writer(static_cast<spk::Message::Type>(type));
		writer.setRequestID(8);
		writer << std::uint8_t{1};
		client.send(std::move(writer).build());
	}
	ASSERT_TRUE(wait(2));
	ASSERT_EQ(received.size(), 2u);
	for (const auto &message : received)
	{
		EXPECT_EQ(message.requestID(), 8u);
		if (message.type() == static_cast<spk::Message::Type>(Networking::MessageType::ChunkError))
		{
			EXPECT_EQ(Networking::ChunkProtocol::Error(message).diagnostic().message, "Chunk_Request_Malformed");
		}
		else
		{
			EXPECT_EQ(Networking::ColumnProtocol::Error(message).diagnostic().message, "Column_Request_Malformed");
		}
	}
}
TEST_F(TerrainHandlers, DuplicatesWarnBeforeOneCanonicalResponse)
{
	client.send(Networking::ColumnProtocol::Request::build(6, {{3, 3}, {3, 3}, {4, 4}}));
	ASSERT_TRUE(wait(2));
	ASSERT_EQ(received.size(), 2u);
	auto error = Networking::ColumnProtocol::Error(received[0]);
	EXPECT_EQ(error.diagnostic().severity, Networking::Diagnostic::Severity::Warning);
	EXPECT_EQ(error.keys(), (std::vector<Column::Coordinate>{{3, 3}}));
	auto response = Networking::ColumnProtocol::Response(received[1]);
	EXPECT_EQ(response.section(0).success.size(), 2u);
}
TEST_F(TerrainHandlers, TwoClientsUsingSameRequestIDReceiveOnlyTheirOwnValues)
{
	spk::Client second;
	second.connect("127.0.0.1", router->port());
	client.send(Networking::ColumnProtocol::Request::build(1, {{3, 3}}));
	second.send(Networking::ColumnProtocol::Request::build(1, {{4, 4}}));
	ASSERT_TRUE(wait(1));
	std::vector<spk::Message> secondMessages;
	const auto deadline = std::chrono::steady_clock::now() + std::chrono::seconds(2);
	while (secondMessages.empty() == true && std::chrono::steady_clock::now() < deadline)
	{
		pump();
		(void)second.messages().drain(secondMessages);
	}
	ASSERT_EQ(secondMessages.size(), 1u);
	EXPECT_EQ(Networking::ColumnProtocol::Response(received.front()).section(0).success.front().element.chunks.size(), 2u);
	EXPECT_EQ(Networking::ColumnProtocol::Response(secondMessages.front()).section(0).success.front().element.chunks.size(), 4u);
	second.disconnect();
}
