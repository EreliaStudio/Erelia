#include "erelia/client/connection_manager.hpp"
#include "erelia/client/service.hpp"
#include "erelia/server/router.hpp"

#include <core/context/update_context.hpp>
#include <gtest/gtest.h>
#include <input/device_context.hpp>
#include <network/client.hpp>

#include <chrono>
#include <thread>

using namespace std::chrono_literals;

namespace
{
	template <typename TPredicate>
	[[nodiscard]] bool waitUntilConnection(
		TPredicate predicate,
		std::chrono::milliseconds timeout = 2s)
	{
		const auto deadline =
			std::chrono::steady_clock::now() + timeout;
		while (std::chrono::steady_clock::now() < deadline)
		{
			if (predicate() == true)
			{
				return true;
			}
			std::this_thread::sleep_for(5ms);
		}
		return predicate();
	}

	void advanceConnectionManager(ConnectionManager &manager)
	{
		spk::UpdateContext context{.time = {}, .deltaTime = 5ms};
		spk::Keyboard keyboard;
		spk::Mouse mouse;
		spk::DeviceContext devices{.keyboard = keyboard, .mouse = mouse};
		manager.updateState(context);
		manager.updateState(context, devices);
	}
}

TEST(ClientServerConnectionIntegration, ConnectionManagerConnectsToRouter)
{
	Router router(
		Router::Configuration{
			.port = 0,
			.nodeReconnectDelay = 10ms,
			.nodes = {}});
	router.start();
	ASSERT_TRUE(router.isRunning());

	ConnectionManager manager(
		"ConnectionManager",
		ConnectionManager::Endpoint{
			.address = "127.0.0.1",
			.port = router.port()},
		5ms);
	spk::Client &client = *Service::client();

	EXPECT_TRUE(
		waitUntilConnection(
			[&] {
				advanceConnectionManager(manager);
				return client.isConnected();
			}));

	router.stop();

	EXPECT_TRUE(
		waitUntilConnection(
			[&] {
				advanceConnectionManager(manager);
				return client.isConnected() == false;
			}));
}
