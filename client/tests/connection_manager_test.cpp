#include "erelia/client/connection_manager.hpp"

#include "erelia/core/service.hpp"

#include <core/context/update_context.hpp>
#include <diagnostics/logger.hpp>
#include <input/device_context.hpp>
#include <network/remote_node.hpp>
#include <threading/worker_pool.hpp>

#include <gtest/gtest.h>

#include <atomic>
#include <chrono>
#include <cstddef>
#include <cstdint>
#include <memory>
#include <string>
#include <thread>
#include <vector>

using namespace std::chrono_literals;

namespace
{
	constexpr std::chrono::milliseconds TestRetryDelay{1};
	constexpr std::chrono::milliseconds TestTimeout{2'000};
	constexpr std::chrono::milliseconds ConnectionCycleTimeout{15'000};
	constexpr std::chrono::milliseconds PollInterval{1};

	template <typename TPredicate>
	[[nodiscard]] bool waitUntil(
		TPredicate predicate,
		std::chrono::milliseconds timeout = TestTimeout)
	{
		const auto deadline =
			std::chrono::steady_clock::now() + timeout;

		while (std::chrono::steady_clock::now() < deadline)
		{
			if (predicate() == true)
			{
				return true;
			}

			std::this_thread::sleep_for(PollInterval);
		}

		return predicate();
	}

	[[nodiscard]] ConnectionManager::Endpoint refusedEndpoint()
	{
		spk::RemoteNode::Endpoint probe;
		probe.start(0);

		const std::uint16_t port = probe.port();

		probe.stop();

		return ConnectionManager::Endpoint{
			.address = "127.0.0.1",
			.port = port};
	}

	void advanceConnectionManager(ConnectionManager &manager)
	{
		spk::UpdateContext context{.time = {}, .deltaTime = {}};
		spk::Keyboard keyboard;
		spk::Mouse mouse;
		spk::DeviceContext devices{.keyboard = keyboard, .mouse = mouse};

		manager.updateState(context);
		manager.updateState(context, devices);
	}

	[[nodiscard]] bool advanceUntilStopped(
		ConnectionManager &manager)
	{
		return waitUntil(
			[&manager] {
				advanceConnectionManager(manager);
				return manager.isCycleStopped();
			},
			ConnectionCycleTimeout);
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
				Service::workerPool();
			_workerCount = workerPool.workerCount();

			for (
				std::size_t index = 0u;
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
					return
						_started->load(
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

	class ConnectionManagerTest : public ::testing::Test
	{
	protected:
		ConnectionManager::Endpoint _endpoint;

		void SetUp() override
		{
			_endpoint = refusedEndpoint();
		}
	};
}

TEST_F(ConnectionManagerTest, ConstructionStartsFirstConnectionAttempt)
{
	ConnectionManager manager(
		"ConnectionManager",
		_endpoint,
		TestRetryDelay);

	EXPECT_EQ(manager.attemptCount(), 1u);
	EXPECT_FALSE(manager.isCycleStopped());
}

TEST_F(ConnectionManagerTest, FailedCycleStopsAfterExactlyThreeAttempts)
{
	std::vector<std::pair<spk::Logger::Level, std::string>> entries;
	auto contract = spk::logger.subscribeToEntry(
		[&entries](const spk::Logger::Level &level, const std::string &message) {
			entries.emplace_back(level, message);
		});

	ConnectionManager manager(
		"ConnectionManager",
		_endpoint,
		TestRetryDelay);

	ASSERT_TRUE(advanceUntilStopped(manager));
	EXPECT_EQ(
		manager.attemptCount(),
		ConnectionManager::MaximumAttemptCount);

	bool stopMessageFound = false;
	for (const auto &[level, message] : entries)
	{
		if (
			level == spk::Logger::Level::Error &&
			message.find(
				"automatic connection attempts stopped") !=
				std::string::npos)
		{
			stopMessageFound = true;
		}
	}
	EXPECT_TRUE(stopMessageFound);

	for (std::size_t iteration = 0u; iteration < 10u; ++iteration)
	{
		advanceConnectionManager(manager);
	}
	EXPECT_EQ(
		manager.attemptCount(),
		ConnectionManager::MaximumAttemptCount);
}

TEST_F(ConnectionManagerTest, ConnectStartsFreshCycleAfterBudgetIsExhausted)
{
	ConnectionManager manager(
		"ConnectionManager",
		_endpoint,
		TestRetryDelay);

	ASSERT_TRUE(advanceUntilStopped(manager));
	ASSERT_EQ(
		manager.attemptCount(),
		ConnectionManager::MaximumAttemptCount);

	manager.connect();

	EXPECT_FALSE(manager.isCycleStopped());
	EXPECT_EQ(manager.attemptCount(), 1u);
}

TEST_F(ConnectionManagerTest, ConnectDoesNotLaunchConcurrentAttempt)
{
	WorkerPoolBlocker workerPoolBlocker;
	ASSERT_TRUE(workerPoolBlocker.waitUntilBlocked());

	ConnectionManager manager(
		"ConnectionManager",
		_endpoint,
		TestRetryDelay);
	ASSERT_EQ(manager.attemptCount(), 1u);

	manager.connect();

	EXPECT_EQ(manager.attemptCount(), 1u);

	workerPoolBlocker.release();
}

TEST_F(ConnectionManagerTest, ValidationExceptionRemainsStable)
{
	try
	{
		ConnectionManager manager(
			"ConnectionManager",
			{"127.0.0.1", 0},
			TestRetryDelay);
		FAIL() << "Expected spk::Exception";
	} catch (const spk::Exception &exception)
	{
		EXPECT_EQ(
			exception.message(),
			"Client Server port cannot be zero");
	}
}

TEST_F(ConnectionManagerTest, ConnectionAttemptDiagnosticRemainsStable)
{
	std::vector<std::pair<spk::Logger::Level, std::string>> entries;
	auto contract = spk::logger.subscribeToEntry(
		[&entries](const spk::Logger::Level &level, const std::string &message) {
			entries.emplace_back(level, message);
		});

	ConnectionManager manager(
		"ConnectionManager",
		_endpoint,
		TestRetryDelay);

	bool diagnosticFound = false;
	for (const auto &[level, message] : entries)
	{
		if (
			level == spk::Logger::Level::Info &&
			message ==
				"Connecting to dedicated Server (attempt 1/" +
					std::to_string(
						ConnectionManager::MaximumAttemptCount) +
					')')
		{
			diagnosticFound = true;
		}
	}

	EXPECT_TRUE(diagnosticFound);
}
