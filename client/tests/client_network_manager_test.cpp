#include "erelia/client/client_network_manager.hpp"
#include "erelia/client/client_world.hpp"
#include "erelia/client/service.hpp"
#include <chrono>
#include <gtest/gtest.h>
#include <network/client.hpp>
#include <network/server.hpp>
#include <thread>
TEST(ClientNetworkManager, UniqueDrainerAndMultipleSubscriptions)
{
	ClientNetworkManager manager;
	EXPECT_THROW((void)ClientNetworkManager{}, spk::Exception);
	int calls = 0;
	auto first = manager.dispatcher().subscribe(45, [&](const auto &) {
		++calls;
	});
	{
		auto second = manager.dispatcher().subscribe(45, [&](const auto &) {
			++calls;
		});
		spk::Message::Writer writer(45);
		Service::client().messages().publish(std::move(writer).build());
		manager.dispatch();
	}
	EXPECT_EQ(calls, 2);
	spk::Message::Writer writer(45);
	Service::client().messages().publish(std::move(writer).build());
	manager.dispatch();
	EXPECT_EQ(calls, 3);
	std::vector<spk::Message> remaining;
	EXPECT_TRUE(Service::client().messages().drain(remaining).empty());
}
TEST(ClientNetworkManager, ServiceDisconnectEventFailsPendingAndPreservesAvailable)
{
	Service::client().disconnect();
	spk::Server server;
	server.start(0);
	Service::client().connect("127.0.0.1", server.port());
	ClientNetworkManager manager;
	ClientWorld world(manager);
	world.chunkCollection()->insert({1, 0, 0}, Chunk{});
	auto pending = world.columnCollection()->request(Column::Coordinate{2, 0});
	Service::client().disconnect();
	Service::clientEventCenter().clientDisconnected().trigger();
	manager.dispatch();
	EXPECT_EQ(pending.status(), spk::Task<Column>::Status::Failed);
	EXPECT_EQ(world.chunkCollection()->state({1, 0, 0}), World::Chunks::State::Available);
	server.stop();
}
