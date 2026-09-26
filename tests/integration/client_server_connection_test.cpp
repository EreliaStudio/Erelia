#include "erelia/client/client_runtime.hpp"
#include "erelia/server/router.hpp"

#include <design_pattern/singleton.hpp>
#include <gtest/gtest.h>
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

	void ensureConnectionWorkerPool()
	{
		if (
			spk::Singleton<spk::WorkerPool>::isInstanciated() ==
			false)
		{
			spk::Singleton<spk::WorkerPool>::instanciate(
				new spk::WorkerPool());
		}
	}
}

TEST(ClientServerConnectionIntegration, EreliaClientRuntimeConnectsToRouter)
{
	ensureConnectionWorkerPool();

	Router router(
		Router::Configuration{
			.port = 0,
			.nodeReconnectDelay = 10ms,
			.nodes = {}});
	router.start();
	ASSERT_TRUE(router.isRunning());

	ClientRuntime client(
		ClientRuntime::Configuration{
			.address = "127.0.0.1",
			.port = router.port()});

	ClientRuntime::ConnectionAnswer connection =
		client.connect();
	EXPECT_TRUE(connection.get());
	EXPECT_TRUE(client.isConnected());

	router.stop();

	EXPECT_TRUE(
		waitUntilConnection(
			[&client] {
				return client.isConnected() == false;
			}));

	client.disconnect();
}
