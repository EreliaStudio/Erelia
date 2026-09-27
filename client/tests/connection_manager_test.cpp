#include "erelia/client/connection_manager.hpp"

#include <core/context/update_context.hpp>
#include <diagnostics/logger.hpp>
#include <input/device_context.hpp>

#include <gtest/gtest.h>

#include <chrono>
#include <string>
#include <thread>
#include <vector>

namespace
{
	void advanceConnectionManager(ConnectionManager &manager, std::chrono::steady_clock::duration delta)
	{
		spk::UpdateContext context{.time = {}, .deltaTime = delta};
		spk::Keyboard keyboard;
		spk::Mouse mouse;
		spk::DeviceContext devices{.keyboard = keyboard, .mouse = mouse};
		manager.updateState(context);
		manager.updateState(context, devices);
	}

	bool advanceUntilStopped(ConnectionManager &manager)
	{
		for (std::size_t iteration = 0; iteration < 400 && manager.isCycleStopped() == false; ++iteration)
		{
			advanceConnectionManager(manager, ConnectionManager::RetryDelay);
			std::this_thread::sleep_for(std::chrono::milliseconds(5));
		}
		return manager.isCycleStopped();
	}

	class ConnectionManagerTest : public ::testing::Test
	{
	};
}

TEST_F(ConnectionManagerTest, ConstructionStartsFirstConnectionAttempt)
{
	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
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

	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
	ASSERT_TRUE(advanceUntilStopped(manager));
	EXPECT_EQ(manager.attemptCount(), ConnectionManager::MaximumAttemptCount);

	bool stopMessageFound = false;
	for (const auto &[level, message] : entries)
	{
		if (
			level == spk::Logger::Level::Error &&
			message.find("automatic connection attempts stopped") != std::string::npos)
		{
			stopMessageFound = true;
		}
	}
	EXPECT_TRUE(stopMessageFound);

	for (std::size_t iteration = 0; iteration < 10; ++iteration)
	{
		advanceConnectionManager(manager, ConnectionManager::RetryDelay);
	}
	EXPECT_EQ(manager.attemptCount(), ConnectionManager::MaximumAttemptCount);
}

TEST_F(ConnectionManagerTest, ConnectStartsFreshCycleAfterBudgetIsExhausted)
{
	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
	ASSERT_TRUE(advanceUntilStopped(manager));
	ASSERT_EQ(manager.attemptCount(), ConnectionManager::MaximumAttemptCount);

	manager.connect();

	EXPECT_FALSE(manager.isCycleStopped());
	EXPECT_EQ(manager.attemptCount(), 1u);
}

TEST_F(ConnectionManagerTest, ConnectDoesNotLaunchConcurrentAttempt)
{
	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
	ASSERT_EQ(manager.attemptCount(), 1u);

	manager.connect();

	EXPECT_EQ(manager.attemptCount(), 1u);
}
