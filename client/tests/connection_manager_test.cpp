#include "erelia/client/connection_manager.hpp"
#include "erelia/client/service.hpp"

#include <core/context/update_context.hpp>
#include <diagnostics/logger.hpp>
#include <input/device_context.hpp>
#include <system/translator.hpp>

#include <gtest/gtest.h>

#include <chrono>
#include <string>
#include <thread>
#include <vector>

namespace
{
	constexpr std::chrono::milliseconds TestRetryDelay{1};

	void advanceConnectionManager(ConnectionManager &manager)
	{
		spk::UpdateContext context{.time = {}, .deltaTime = {}};
		spk::Keyboard keyboard;
		spk::Mouse mouse;
		spk::DeviceContext devices{.keyboard = keyboard, .mouse = mouse};
		manager.updateState(context);
		manager.updateState(context, devices);
	}

	bool advanceUntilStopped(ConnectionManager &manager)
	{
		for (
			std::size_t iteration = 0;
			iteration < 400 && manager.isCycleStopped() == false;
			++iteration)
		{
			advanceConnectionManager(manager);
			std::this_thread::sleep_for(std::chrono::milliseconds(2));
		}
		return manager.isCycleStopped();
	}

	class ConnectionManagerTest : public ::testing::Test
	{
	protected:
		void SetUp() override
		{
			Service::translator()->clear();
		}

		void TearDown() override
		{
			Service::translator()->clear();
		}
	};
}

TEST_F(ConnectionManagerTest, ConstructionStartsFirstConnectionAttempt)
{
	ConnectionManager manager(
		"ConnectionManager",
		{"127.0.0.1", 1},
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
		{"127.0.0.1", 1},
		TestRetryDelay);
	ASSERT_TRUE(advanceUntilStopped(manager));
	EXPECT_EQ(manager.attemptCount(), ConnectionManager::MaximumAttemptCount);

	bool stopMessageFound = false;
	for (const auto &[level, message] : entries)
	{
		if (
			level == spk::Logger::Level::Error &&
			message == "client.connection.maximum_attempts_reached")
		{
			stopMessageFound = true;
		}
	}
	EXPECT_TRUE(stopMessageFound);

	for (std::size_t iteration = 0; iteration < 10; ++iteration)
	{
		advanceConnectionManager(manager);
	}
	EXPECT_EQ(manager.attemptCount(), ConnectionManager::MaximumAttemptCount);
}

TEST_F(ConnectionManagerTest, ConnectStartsFreshCycleAfterBudgetIsExhausted)
{
	ConnectionManager manager(
		"ConnectionManager",
		{"127.0.0.1", 1},
		TestRetryDelay);
	ASSERT_TRUE(advanceUntilStopped(manager));
	ASSERT_EQ(manager.attemptCount(), ConnectionManager::MaximumAttemptCount);

	manager.connect();

	EXPECT_FALSE(manager.isCycleStopped());
	EXPECT_EQ(manager.attemptCount(), 1u);
}

TEST_F(ConnectionManagerTest, ConnectDoesNotLaunchConcurrentAttempt)
{
	ConnectionManager manager(
		"ConnectionManager",
		{"127.0.0.1", 1},
		TestRetryDelay);
	ASSERT_EQ(manager.attemptCount(), 1u);

	manager.connect();

	EXPECT_EQ(manager.attemptCount(), 1u);
}


TEST_F(ConnectionManagerTest, UsesRegisteredTranslationForConnectionAttempt)
{
	Service::translator()->append(
		"client.connection.attempt",
		"Attempt {}/{}");

	std::vector<std::pair<spk::Logger::Level, std::string>> entries;
	auto contract = spk::logger.subscribeToEntry(
		[&entries](const spk::Logger::Level &level, const std::string &message) {
			entries.emplace_back(level, message);
		});

	ConnectionManager manager(
		"ConnectionManager",
		{"127.0.0.1", 1},
		TestRetryDelay);

	bool translatedAttemptFound = false;
	for (const auto &[level, message] : entries)
	{
		if (
			level == spk::Logger::Level::Info &&
			message ==
				"Attempt 1/" +
				std::to_string(ConnectionManager::MaximumAttemptCount))
		{
			translatedAttemptFound = true;
		}
	}

	EXPECT_TRUE(translatedAttemptFound);
}


TEST_F(ConnectionManagerTest, UsesTranslatedEndpointValidationMessage)
{
	Service::translator()->append(
		"client.connection.endpoint.port_zero",
		"Translated zero port");

	try
	{
		ConnectionManager manager(
			"ConnectionManager",
			{"127.0.0.1", 0},
			TestRetryDelay);
		FAIL() << "Expected spk::Exception";
	}
	catch (const spk::Exception &exception)
	{
		EXPECT_STREQ(exception.what(), "Translated zero port");
	}
}
