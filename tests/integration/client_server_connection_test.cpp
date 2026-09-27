#include "erelia/client/connection_manager.hpp"
#include "erelia/server/router.hpp"

#include <core/context/update_context.hpp>
#include <design_pattern/singleton.hpp>
#include <gtest/gtest.h>
#include <input/device_context.hpp>
#include <network/client.hpp>
#include <threading/worker_pool.hpp>

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

	void ensureConnectionServices()
	{
		if (
			spk::Singleton<spk::WorkerPool>::isInstanciated() ==
			false)
		{
			spk::Singleton<spk::WorkerPool>::instanciate(
				new spk::WorkerPool());
		}
		if (spk::Singleton<spk::Client>::isInstanciated() == false)
		{
			spk::Singleton<spk::Client>::instanciate(new spk::Client());
		}
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
	ensureConnectionServices();

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
			.port = router.port()});
	spk::Client &client = spk::Singleton<spk::Client>::instance();

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
