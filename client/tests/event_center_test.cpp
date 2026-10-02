#include "erelia/client/service.hpp"
#include "erelia/core/service.hpp"

#include <gtest/gtest.h>
#include <string>
#include <thread>
#include <type_traits>
#include <vector>

static_assert(std::is_base_of_v<Core::EventCenter, Client::EventCenter> == false);

TEST(EventCenter, ServiceInstanceIsStableAndEventsAreIndependent)
{
	EXPECT_EQ(&Service::clientEventCenter(), &Service::clientEventCenter());
	EXPECT_EQ(&Service::coreEventCenter(), &Service::coreEventCenter());
	int connected = 0;
	int disconnected = 0;
	auto first = Service::clientEventCenter().clientConnected().subscribe([&] {
		++connected;
	});
	auto second = Service::clientEventCenter().clientDisconnected().subscribe([&] {
		++disconnected;
	});
	Service::clientEventCenter().clientConnected().trigger();
	EXPECT_EQ(connected, 1);
	EXPECT_EQ(disconnected, 0);
	Service::clientEventCenter().clientDisconnected().trigger();
	EXPECT_EQ(connected, 1);
	EXPECT_EQ(disconnected, 1);
}

TEST(EventCenter, SubscriptionLifetimeAndTypedPayload)
{
	Client::EventCenter events;
	std::vector<spk::Vector3Int> positions;
	auto first = events.playerChangedChunkEvent().subscribe([&](spk::Vector3Int position) {
		positions.push_back(position);
	});
	{
		auto second = events.playerChangedChunkEvent().subscribe([&](spk::Vector3Int position) {
			positions.push_back(position);
		});
		events.playerChangedChunkEvent().trigger({-1, 2, 3});
	}
	events.playerChangedChunkEvent().trigger({4, 5, 6});
	first.resign();
	events.playerChangedChunkEvent().trigger({7, 8, 9});
	EXPECT_EQ(positions, (std::vector<spk::Vector3Int>{{-1, 2, 3}, {-1, 2, 3}, {4, 5, 6}}));
}

TEST(EventCenter, EventSupportsOtherPayloadsAndRunsOnEmittingThread)
{
	Core::Event<std::string, int> event;
	std::string received;
	int count = 0;
	std::thread::id caller;
	auto contract = event.subscribe([&](std::string value, int number) {
		received = std::move(value);
		count = number;
		caller = std::this_thread::get_id();
	});
	std::thread emitter([&] {
		event.trigger("Other event", 42);
	});
	const auto emitterID = emitter.get_id();
	emitter.join();
	EXPECT_EQ(received, "Other event");
	EXPECT_EQ(count, 42);
	EXPECT_EQ(caller, emitterID);
}
