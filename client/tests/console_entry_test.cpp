#include "erelia/client/connection_manager.hpp"
#include "erelia/client/console_entry.hpp"

#include <design_pattern/singleton.hpp>
#include <diagnostics/logger.hpp>
#include <network/client.hpp>
#include <threading/worker_pool.hpp>

#include <gtest/gtest.h>

#include <string>
#include <vector>

namespace
{
	void ensureClientServices()
	{
		if (spk::Singleton<spk::WorkerPool>::isInstanciated() == false)
		{
			spk::Singleton<spk::WorkerPool>::instanciate(new spk::WorkerPool());
		}
		if (spk::Singleton<spk::Client>::isInstanciated() == false)
		{
			spk::Singleton<spk::Client>::instanciate(new spk::Client());
		}
	}

	class ConsoleEntryTest : public ::testing::Test
	{
	protected:
		static void SetUpTestSuite()
		{
			ensureClientServices();
		}
	};
}

TEST_F(ConsoleEntryTest, OrdinaryTextUsesLogger)
{
	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
	std::vector<std::string> local;
	spk::Logger::Level level = spk::Logger::Level::Trace;
	std::string message;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &receivedLevel, const std::string &receivedMessage) {
			level = receivedLevel;
			message = receivedMessage;
		});
	ConsoleEntry entry("Entry", manager, [&](std::string value) {
		local.emplace_back(std::move(value));
	});

	entry.submit("hello");

	EXPECT_EQ(level, spk::Logger::Level::UserValueA);
	EXPECT_EQ(message, "hello");
	EXPECT_TRUE(local.empty());
}

TEST_F(ConsoleEntryTest, GlobalHelpIsLocalAndDoesNotUseLogger)
{
	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
	std::vector<std::string> local;
	std::size_t loggerCalls = 0;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &, const std::string &) {
			++loggerCalls;
		});
	ConsoleEntry entry("Entry", manager, [&](std::string value) {
		local.emplace_back(std::move(value));
	});
	loggerCalls = 0;

	entry.submit("/help");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_NE(local.front().find("/connect"), std::string::npos);
	EXPECT_EQ(loggerCalls, 0u);
}

TEST_F(ConsoleEntryTest, CommandHelpIsLocalAndDoesNotExecuteCommand)
{
	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
	const std::size_t attempts = manager.attemptCount();
	std::vector<std::string> local;
	ConsoleEntry entry("Entry", manager, [&](std::string value) {
		local.emplace_back(std::move(value));
	});

	entry.submit("/connect --help");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_NE(local.front().find("Usage: /connect"), std::string::npos);
	EXPECT_EQ(manager.attemptCount(), attempts);
}

TEST_F(ConsoleEntryTest, MalformedKnownCommandAddsDiagnosticAndUsageLocally)
{
	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
	std::vector<std::string> local;
	std::size_t loggerCalls = 0;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &, const std::string &) {
			++loggerCalls;
		});
	ConsoleEntry entry("Entry", manager, [&](std::string value) {
		local.emplace_back(std::move(value));
	});
	loggerCalls = 0;

	entry.submit("/connect --unknown value");

	ASSERT_EQ(local.size(), 2u);
	EXPECT_EQ(local[0], "Unknown parameter: --unknown");
	EXPECT_NE(local[1].find("Usage: /connect"), std::string::npos);
	EXPECT_EQ(loggerCalls, 0u);
}

TEST_F(ConsoleEntryTest, UnknownCommandReportsLocallyWithoutLogger)
{
	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
	std::vector<std::string> local;
	std::size_t loggerCalls = 0;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &, const std::string &) {
			++loggerCalls;
		});
	ConsoleEntry entry("Entry", manager, [&](std::string value) {
		local.emplace_back(std::move(value));
	});
	loggerCalls = 0;

	entry.submit("/missing");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_EQ(local.front(), "Unknown command: /missing");
	EXPECT_EQ(loggerCalls, 0u);
}
