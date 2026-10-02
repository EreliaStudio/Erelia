#include "erelia/client/connection_manager.hpp"
#include "erelia/client/service.hpp"
#include "erelia/client/terrain_collections.hpp"
#include "erelia/client/terrain_streaming_behaviour.hpp"
#include "erelia/server/router.hpp"
#include "terrain_node.hpp"
#include <chrono>
#include <core/context/update_context.hpp>
#include <gtest/gtest.h>
#include <thread>
namespace
{
	using namespace std::chrono_literals;
	class RoutedTerrain : public testing::Test
	{
	protected:
		TerrainNode terrain{{.port = 0}};
		std::unique_ptr<Router> router;
		ClientNetworkManager manager;
		TerrainCollections client{manager};
		std::unique_ptr<ConnectionManager> connection;
		void updateConnection()
		{
			spk::UpdateContext context{.time = {}, .deltaTime = 1ms};
			connection->updateState(context);
		}
		void SetUp() override
		{
			Service::client().disconnect();
			manager.dispatch();
			terrain.start();
			router = std::make_unique<Router>(Router::Configuration{.port = 0, .nodeReconnectDelay = 1ms, .nodes = {{.name = "terrain", .address = "127.0.0.1", .port = terrain.port()}}});
			router->redirect(static_cast<spk::Message::Type>(Networking::MessageType::ChunkRequest), "terrain");
			router->redirect(static_cast<spk::Message::Type>(Networking::MessageType::ColumnRequest), "terrain");
			router->start();
			connection = std::make_unique<ConnectionManager>("Connection", ConnectionManager::Endpoint{"127.0.0.1", router->port()}, 1ms);
			ASSERT_TRUE(waitForConnection());
		}
		void TearDown() override
		{
			connection.reset();
			manager.dispatch();
			router->stop();
			terrain.stop();
		}
		void pump()
		{
			updateConnection();
			router->dispatch();
			terrain.dispatch();
			router->dispatch();
			manager.dispatch();
		}
		bool waitForConnection()
		{
			const auto deadline = std::chrono::steady_clock::now() + 2s;
			while (std::chrono::steady_clock::now() < deadline)
			{
				pump();
				if (Service::client().isConnected() == true && connection->attemptCount() == 0)
				{
					return true;
				}
				std::this_thread::yield();
			}
			return false;
		}
		template <typename TAnswer>
		bool wait(const TAnswer &answer)
		{
			const auto deadline = std::chrono::steady_clock::now() + 2s;
			while (answer.status() == decltype(answer.status())::Pending && std::chrono::steady_clock::now() < deadline)
			{
				pump();
				std::this_thread::yield();
			}
			return answer.status() != decltype(answer.status())::Pending;
		}
	};
}
TEST_F(RoutedTerrain, CanonicalSingleAndMultiChunkAcquisitionUsesClientCollections)
{
	auto group = client.chunks().request(std::vector<Chunk::Coordinate>{{0, 0, 0}, {1, 0, 1}, {-1, 0, -1}, {0, -1, 0}, {2, 0, 1}});
	ASSERT_TRUE(wait(group));
	ASSERT_EQ(group.status(), spk::Task<Chunk>::Status::Completed);
	EXPECT_EQ(group.at(0).result().at({0, 3, 0}).definitionId(), 1u);
	EXPECT_EQ(group.at(1).result().at({4, 1, 4}).definitionId(), 2u);
	EXPECT_EQ(group.at(2).result().at({0, 0, 0}).definitionId(), 1u);
	EXPECT_EQ(group.at(4).result().at({4, 1, 4}).definitionId(), 3u);
	const auto baseline = group.at(1).result().at({3, 0, 8});
	EXPECT_EQ(baseline.definitionId(), 1u);
	EXPECT_EQ(baseline.orientation(), Voxel::Cell::Orientation::PositiveX);
	EXPECT_EQ(baseline.flipOrientation(), Voxel::Cell::FlipOrientation::PositiveY);
	const auto slope = group.at(1).result().at({4, 1, 4});
	EXPECT_EQ(slope.orientation(), Voxel::Cell::Orientation::PositiveX);
	EXPECT_EQ(slope.flipOrientation(), Voxel::Cell::FlipOrientation::PositiveY);
	EXPECT_EQ(group.at(1).result().at({3, 1, 8}).packed(), Voxel::Cell::Empty.packed());
	for (const auto &cell : group.at(3).result().cells())
	{
		EXPECT_EQ(cell.packed(), 0u);
	}
	const auto cached = client.chunks().request(Chunk::Coordinate{1, 0, 1});
	EXPECT_EQ(cached.status(), spk::Task<Chunk>::Status::Completed);
}
TEST_F(RoutedTerrain, ColumnsAndChunksUseIndependentRequestIDOne)
{
	auto column = client.columns().request(Column::Coordinate{4, 4});
	auto chunk = client.chunks().request(Chunk::Coordinate{4, 3, 4});
	ASSERT_TRUE(wait(column));
	ASSERT_TRUE(wait(chunk));
	ASSERT_EQ(column.result().chunks.size(), 4u);
	for (int y = 0; y < 4; ++y)
	{
		EXPECT_EQ(column.result().chunks[static_cast<std::size_t>(y)], (Chunk::Coordinate{4, y, 4}));
	}
	for (const auto &cell : chunk.result().cells())
	{
		EXPECT_EQ(cell.packed(), 1u);
	}
}
TEST_F(RoutedTerrain, PlayerColumnChunkChainAndUnloadUsesRealNetwork)
{
	spk::Entity3D player("Player");
	player.transform().place({64, 0, 64});
	auto &streaming = player.addBehaviour<TerrainStreamingBehaviour>(client.columns(), client.chunks(), TerrainStreamingBehaviour::Ranges{1, 2});
	const auto deadline = std::chrono::steady_clock::now() + 2s;
	while (client.chunks().state({4, 3, 4}) != TerrainCollections::Chunks::State::Available && std::chrono::steady_clock::now() < deadline)
	{
		pump();
		streaming.dispatch();
		std::this_thread::yield();
	}
	ASSERT_EQ(client.chunks().state({4, 3, 4}), TerrainCollections::Chunks::State::Available);
	player.transform().place({-64, 0, -64});
	EXPECT_EQ(client.chunks().state({4, 3, 4}), TerrainCollections::Chunks::State::Absent);
}
TEST_F(RoutedTerrain, DisconnectFailsPendingPreservesAvailableAndReconnectCanAcquire)
{
	client.chunks().insert({1, 0, 0}, Chunk{});
	auto pending = client.columns().request(Column::Coordinate{3, 3});
	Service::client().disconnect();
	updateConnection();
	EXPECT_EQ(pending.status(), spk::Task<Column>::Status::Failed);
	EXPECT_EQ(client.chunks().state({1, 0, 0}), TerrainCollections::Chunks::State::Available);
	ASSERT_TRUE(waitForConnection());
	auto current = client.columns().request(Column::Coordinate{3, 3});
	ASSERT_TRUE(wait(current));
	ASSERT_EQ(current.status(), spk::Task<Column>::Status::Completed);
	EXPECT_EQ(current.result().chunks.size(), 2u);
}
TEST_F(RoutedTerrain, RemovedPendingCannotBeRepublishedByRoutedResponse)
{
	auto pending = client.chunks().request(Chunk::Coordinate{7, 0, 7});
	client.chunks().remove({7, 0, 7});
	EXPECT_EQ(pending.status(), spk::Task<Chunk>::Status::Failed);
	auto healthy = client.chunks().request(Chunk::Coordinate{8, 0, 8});
	ASSERT_TRUE(wait(healthy));
	EXPECT_EQ(client.chunks().state({7, 0, 7}), TerrainCollections::Chunks::State::Absent);
}
