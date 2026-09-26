#include "erelia/client/application.hpp"
#include "erelia/client/client_runtime.hpp"

#include <design_pattern/singleton.hpp>
#include <exception.hpp>
#include <gtest/gtest.h>
#include <network/server.hpp>
#include <threading/worker_pool.hpp>
#include <type/uuid.hpp>

#include <atomic>
#include <chrono>
#include <csignal>
#include <cstdlib>
#include <cstddef>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <memory>
#include <string>
#include <thread>

using namespace std::chrono_literals;

namespace
{
	class TemporaryJsonFile final
	{
	private:
		std::filesystem::path _path;

	public:
		explicit TemporaryJsonFile(
			const std::string &content)
		{
			_path =
				std::filesystem::temp_directory_path() /
				("erelia-client-" +
				 spk::UUID::generate().toString() +
				 ".json");
			std::ofstream stream(_path);
			stream << content;
		}

		~TemporaryJsonFile()
		{
			std::error_code error;
			std::filesystem::remove(_path, error);
		}

		[[nodiscard]] const std::filesystem::path &path() const noexcept
		{
			return _path;
		}
	};

	template <typename TPredicate>
	[[nodiscard]] bool waitUntil(
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

	void ensureWorkerPool()
	{
		if (
			spk::Singleton<spk::WorkerPool>::isInstanciated() ==
			false)
		{
			spk::Singleton<spk::WorkerPool>::instanciate(
				new spk::WorkerPool());
		}
	}

	[[nodiscard]] std::uint16_t availablePort()
	{
		spk::Server probe;
		probe.start(0);
		const std::uint16_t port = probe.port();
		probe.stop();
		return port;
	}

	class WorkerPoolBlocker final
	{
	private:
		std::shared_ptr<std::atomic_bool> _release =
			std::make_shared<std::atomic_bool>(false);
		std::shared_ptr<std::atomic<std::size_t>> _started =
			std::make_shared<std::atomic<std::size_t>>(0u);
		std::size_t _workerCount = 0u;
		bool _released = false;

	public:
		WorkerPoolBlocker()
		{
			spk::WorkerPool &workerPool =
				spk::Singleton<spk::WorkerPool>::instance();
			_workerCount = workerPool.workerCount();

			for (std::size_t index = 0u;
				 index < _workerCount;
				 ++index)
			{
				(void)workerPool.submit(
					[release = _release,
					 started = _started] {
						started->fetch_add(
							1u,
							std::memory_order_release);
						release->wait(
							false,
							std::memory_order_acquire);
						return true;
					});
			}
		}

		~WorkerPoolBlocker()
		{
			release();
		}

		[[nodiscard]] bool waitUntilBlocked() const
		{
			return waitUntil(
				[this] {
					return _started->load(
							   std::memory_order_acquire) ==
						   _workerCount;
				});
		}

		void release()
		{
			if (_released == true)
			{
				return;
			}

			_released = true;
			_release->store(
				true,
				std::memory_order_release);
			_release->notify_all();
		}
	};
}

TEST(ClientConfiguration, LoadsExactExternalContract)
{
	const TemporaryJsonFile file(
		R"({"server config":{"address":"127.0.0.1","port":2550}})");

	const ClientRuntime::Configuration configuration =
		ClientRuntime::Configuration::load(
			file.path().string());

	EXPECT_EQ(configuration.address, "127.0.0.1");
	EXPECT_EQ(configuration.port, 2550u);
}

TEST(ClientConfiguration, RejectsInvalidContracts)
{
	const std::string fixtures[] = {
		R"({})",
		R"({"server config":{"address":"127.0.0.1"}})",
		R"({"server config":{"port":2550}})",
		R"({"server config":{"address":"","port":2550}})",
		R"({"server config":{"address":"127.0.0.1","port":0}})",
		R"({"server config":{"address":"127.0.0.1","port":70000}})",
		R"({"server config":{"address":"127.0.0.1","port":2550,"extra":true}})",
		R"({"server config":{"address":"127.0.0.1","port":2550},"extra":true})"};

	for (const std::string &fixture : fixtures)
	{
		const TemporaryJsonFile file(fixture);
		EXPECT_THROW(
			(void)ClientRuntime::Configuration::load(
				file.path().string()),
			spk::Exception);
	}
}

TEST(ClientRuntimeConnection, ConnectsAsynchronouslyAndDisconnectsIdempotently)
{
	ensureWorkerPool();

	spk::Server server;
	server.start(0);

	ClientRuntime client(
		ClientRuntime::Configuration{
			.address = "127.0.0.1",
			.port = server.port()});

	EXPECT_FALSE(client.isConnected());

	ClientRuntime::ConnectionAnswer first =
		client.connect();
	EXPECT_TRUE(first.get());
	EXPECT_TRUE(client.isConnected());

	ClientRuntime::ConnectionAnswer repeated =
		client.connect();
	EXPECT_EQ(
		repeated.status(),
		ClientRuntime::ConnectionTask::Status::Completed);
	EXPECT_EQ(
		std::addressof(first.get()),
		std::addressof(repeated.get()));
	EXPECT_TRUE(client.isConnected());

	client.disconnect();
	EXPECT_FALSE(client.isConnected());
	client.disconnect();
	EXPECT_FALSE(client.isConnected());

	server.stop();
}

TEST(ClientRuntimeConnection, ReusesPendingConnectionAttempt)
{
	ensureWorkerPool();

	spk::Server server;
	server.start(0);
	WorkerPoolBlocker blocker;
	ASSERT_TRUE(blocker.waitUntilBlocked());

	ClientRuntime client(
		ClientRuntime::Configuration{
			.address = "127.0.0.1",
			.port = server.port()});

	ClientRuntime::ConnectionAnswer first =
		client.connect();
	ASSERT_EQ(
		first.status(),
		ClientRuntime::ConnectionTask::Status::Pending);

	ClientRuntime::ConnectionAnswer repeated =
		client.connect();
	EXPECT_EQ(
		repeated.status(),
		ClientRuntime::ConnectionTask::Status::Pending);

	blocker.release();

	EXPECT_TRUE(first.get());
	EXPECT_TRUE(repeated.get());
	EXPECT_EQ(
		std::addressof(first.get()),
		std::addressof(repeated.get()));
	EXPECT_TRUE(client.isConnected());

	client.disconnect();
	server.stop();
}

TEST(ClientRuntimeConnection, FailedAttemptLeavesDisconnectedAndCanRetry)
{
	ensureWorkerPool();

	const std::uint16_t port = availablePort();
	ClientRuntime client(
		ClientRuntime::Configuration{
			.address = "127.0.0.1",
			.port = port});

	ClientRuntime::ConnectionAnswer failure =
		client.connect();
	failure.wait();

	EXPECT_EQ(
		failure.status(),
		ClientRuntime::ConnectionTask::Status::Failed);
	EXPECT_THROW((void)failure.get(), spk::Exception);
	EXPECT_FALSE(client.isConnected());

	spk::Server server;
	server.start(port);

	ClientRuntime::ConnectionAnswer retry =
		client.connect();
	EXPECT_TRUE(retry.get());
	EXPECT_TRUE(client.isConnected());

	client.disconnect();
	server.stop();
}

TEST(ClientRuntimeConnection, ExplicitlyReconnectsSameRuntimeAfterDisconnect)
{
	ensureWorkerPool();

	spk::Server server;
	server.start(0);

	ClientRuntime client(
		ClientRuntime::Configuration{
			.address = "127.0.0.1",
			.port = server.port()});

	ClientRuntime::ConnectionAnswer first =
		client.connect();
	EXPECT_TRUE(first.get());
	ASSERT_TRUE(client.isConnected());

	client.disconnect();
	ASSERT_FALSE(client.isConnected());

	ClientRuntime::ConnectionAnswer second =
		client.connect();
	EXPECT_TRUE(second.get());
	EXPECT_TRUE(client.isConnected());

	client.disconnect();
	server.stop();
}

TEST(ClientRuntimeConnection, RemoteDisconnectUpdatesLiveConnectionState)
{
	ensureWorkerPool();

	spk::Server server;
	server.start(0);

	ClientRuntime client(
		ClientRuntime::Configuration{
			.address = "127.0.0.1",
			.port = server.port()});

	ClientRuntime::ConnectionAnswer connection =
		client.connect();
	EXPECT_TRUE(connection.get());
	ASSERT_TRUE(client.isConnected());

	server.stop();

	EXPECT_TRUE(
		waitUntil(
			[&client] {
				return client.isConnected() == false;
			}));

	client.disconnect();
}

TEST(ClientRuntimeConnection, DisconnectWaitsForPendingAttemptBeforeCleanup)
{
	ensureWorkerPool();

	spk::Server server;
	server.start(0);
	WorkerPoolBlocker blocker;
	ASSERT_TRUE(blocker.waitUntilBlocked());

	ClientRuntime client(
		ClientRuntime::Configuration{
			.address = "127.0.0.1",
			.port = server.port()});

	ClientRuntime::ConnectionAnswer connection =
		client.connect();
	ASSERT_EQ(
		connection.status(),
		ClientRuntime::ConnectionTask::Status::Pending);

	std::atomic_bool disconnectStarted = false;
	std::atomic_bool disconnectCompleted = false;
	std::thread disconnectThread(
		[&] {
			disconnectStarted.store(
				true,
				std::memory_order_release);
			client.disconnect();
			disconnectCompleted.store(
				true,
				std::memory_order_release);
		});

	ASSERT_TRUE(
		waitUntil(
			[&] {
				return disconnectStarted.load(
					std::memory_order_acquire);
			}));
	EXPECT_FALSE(
		disconnectCompleted.load(
			std::memory_order_acquire));

	blocker.release();
	disconnectThread.join();

	EXPECT_TRUE(
		disconnectCompleted.load(
			std::memory_order_acquire));
	EXPECT_FALSE(client.isConnected());

	server.stop();
}


TEST(ClientApplication, InitialConnectionFailureReturnsFailure)
{
	ensureWorkerPool();

	const std::uint16_t port = availablePort();
	const TemporaryJsonFile file(
		"{\"server config\":{\"address\":\"127.0.0.1\",\"port\":" +
		std::to_string(port) +
		"}}");

	std::string configurationPath =
		file.path().string();
	char program[] = "EreliaClient";
	char option[] = "--config";
	char *arguments[] = {
		program,
		option,
		configurationPath.data()};

	EXPECT_EQ(
		runClient(3, arguments),
		EXIT_FAILURE);
}

TEST(ClientApplication, SigtermRequestsCleanShutdown)
{
	ensureWorkerPool();

	spk::Server server;
	server.start(0);

	std::atomic_bool connected = false;
	auto connectionContract =
		server.subscribeToConnection(
			[&connected](spk::ConnectionID) {
				connected.store(
					true,
					std::memory_order_release);
			});

	const TemporaryJsonFile file(
		"{\"server config\":{\"address\":\"127.0.0.1\",\"port\":" +
		std::to_string(server.port()) +
		"}}");

	std::atomic_int result = -1;
	std::thread application(
		[&] {
			std::string configurationPath =
				file.path().string();
			char program[] = "EreliaClient";
			char option[] = "-c";
			char *arguments[] = {
				program,
				option,
				configurationPath.data()};

			result.store(
				runClient(3, arguments),
				std::memory_order_release);
		});

	const bool didConnect =
		waitUntil(
			[&connected] {
				return connected.load(
					std::memory_order_acquire);
			});

	if (didConnect == false)
	{
		server.stop();
		application.join();
		FAIL() << "Client did not connect before the shutdown test deadline";
		return;
	}

	EXPECT_EQ(std::raise(SIGTERM), 0);
	application.join();

	EXPECT_EQ(
		result.load(std::memory_order_acquire),
		EXIT_SUCCESS);

	server.stop();
	(void)connectionContract;
}
