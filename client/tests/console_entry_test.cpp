#include "erelia/client/command/connect_command.hpp"
#include "erelia/client/console_entry.hpp"

#include <diagnostics/logger.hpp>

#include <gtest/gtest.h>

#include <optional>
#include <string>
#include <utility>
#include <vector>

TEST(ConsoleEntryTest, OrdinaryTextUsesLogger)
{
	std::vector<std::string> local;
	spk::Logger::Level level = spk::Logger::Level::Trace;
	std::string message;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &receivedLevel, const std::string &receivedMessage) {
			level = receivedLevel;
			message = receivedMessage;
		});
	ConsoleEntry entry("Entry");
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});

	entry.submit("hello");

	EXPECT_EQ(level, spk::Logger::Level::UserValueA);
	EXPECT_EQ(message, "hello");
	EXPECT_TRUE(local.empty());
}

TEST(ConsoleEntryTest, GlobalHelpIsLocalAndDoesNotUseLogger)
{
	std::vector<std::string> local;
	std::size_t loggerCalls = 0;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &, const std::string &) {
			++loggerCalls;
		});
	ConsoleEntry entry("Entry");
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});
	loggerCalls = 0;

	entry.submit("/help");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_NE(local.front().find("/connect"), std::string::npos);
	EXPECT_EQ(loggerCalls, 0u);
}

TEST(ConsoleEntryTest, CommandHelpIsLocalAndDoesNotEmitConnectRequest)
{
	std::vector<std::string> local;
	std::size_t requestCount = 0;
	ConsoleEntry entry("Entry");
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});
	auto contract = entry.subscribeToConnectRequest(
		[&](const ConnectCommand::Request &) {
			++requestCount;
		});

	entry.submit("/connect --help");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_NE(local.front().find("Usage: /connect"), std::string::npos);
	EXPECT_EQ(requestCount, 0u);
}

TEST(ConsoleEntryTest, MalformedKnownCommandAddsDiagnosticAndUsageLocally)
{
	std::vector<std::string> local;
	std::size_t loggerCalls = 0;
	std::size_t requestCount = 0;
	auto loggerContract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &, const std::string &) {
			++loggerCalls;
		});
	ConsoleEntry entry("Entry");
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});
	auto requestContract = entry.subscribeToConnectRequest(
		[&](const ConnectCommand::Request &) {
			++requestCount;
		});
	loggerCalls = 0;

	entry.submit("/connect --unknown value");

	ASSERT_EQ(local.size(), 2u);
	EXPECT_EQ(local[0], "Unknown parameter: --unknown");
	EXPECT_NE(local[1].find("Usage: /connect"), std::string::npos);
	EXPECT_EQ(loggerCalls, 0u);
	EXPECT_EQ(requestCount, 0u);
}

TEST(ConsoleEntryTest, UnknownCommandReportsLocallyWithoutLogger)
{
	std::vector<std::string> local;
	std::size_t loggerCalls = 0;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &, const std::string &) {
			++loggerCalls;
		});
	ConsoleEntry entry("Entry");
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});
	loggerCalls = 0;

	entry.submit("/missing");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_EQ(local.front(), "Unknown command: /missing");
	EXPECT_EQ(loggerCalls, 0u);
}

TEST(ConsoleEntryTest, ConnectParametersAreOptionalAndShownInHelp)
{
	std::vector<std::string> local;
	ConsoleEntry entry("Entry");
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});

	entry.submit("/connect --help");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_NE(local.front().find("--address"), std::string::npos);
	EXPECT_NE(local.front().find("--port"), std::string::npos);
}

TEST(ConsoleEntryTest, ConnectWithoutOverridesEmitsEmptyRequest)
{
	std::optional<ConnectCommand::Request> received;
	ConsoleEntry entry("Entry");
	auto contract = entry.subscribeToConnectRequest(
		[&](const ConnectCommand::Request &request) {
			received = request;
		});

	entry.submit("/connect");

	ASSERT_TRUE(received.has_value());
	EXPECT_FALSE(received->address.has_value());
	EXPECT_FALSE(received->port.has_value());
}

TEST(ConsoleEntryTest, ConnectAddressOverrideEmitsAddressOnly)
{
	std::optional<ConnectCommand::Request> received;
	ConsoleEntry entry("Entry");
	auto contract = entry.subscribeToConnectRequest(
		[&](const ConnectCommand::Request &request) {
			received = request;
		});

	entry.submit("/connect --address 192.0.2.1");

	ASSERT_TRUE(received.has_value());
	ASSERT_TRUE(received->address.has_value());
	EXPECT_EQ(*received->address, "192.0.2.1");
	EXPECT_FALSE(received->port.has_value());
}

TEST(ConsoleEntryTest, ConnectPortOverrideEmitsPortOnly)
{
	std::optional<ConnectCommand::Request> received;
	ConsoleEntry entry("Entry");
	auto contract = entry.subscribeToConnectRequest(
		[&](const ConnectCommand::Request &request) {
			received = request;
		});

	entry.submit("/connect --port 2550");

	ASSERT_TRUE(received.has_value());
	EXPECT_FALSE(received->address.has_value());
	ASSERT_TRUE(received->port.has_value());
	EXPECT_EQ(*received->port, 2550u);
}

TEST(ConsoleEntryTest, ConnectRejectsInvalidPortLocally)
{
	std::vector<std::string> local;
	std::size_t requestCount = 0;
	ConsoleEntry entry("Entry");
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});
	auto contract = entry.subscribeToConnectRequest(
		[&](const ConnectCommand::Request &) {
			++requestCount;
		});

	entry.submit("/connect --port invalid");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_EQ(local.front(), "Invalid port: invalid");
	EXPECT_EQ(requestCount, 0u);
}

TEST(ConsoleEntryTest, ConnectRejectsOutOfRangePortLocally)
{
	std::vector<std::string> local;
	std::size_t requestCount = 0;
	ConsoleEntry entry("Entry");
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});
	auto contract = entry.subscribeToConnectRequest(
		[&](const ConnectCommand::Request &) {
			++requestCount;
		});

	entry.submit("/connect --port 65536");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_EQ(local.front(), "Invalid port: 65536");
	EXPECT_EQ(requestCount, 0u);
}


TEST(ConsoleEntryTest, SubmissionContractReceivesLocalOutput)
{
	ConsoleEntry entry("Entry");
	std::vector<std::string> submissions;
	auto contract = entry.subscribeToSubmission(
		[&](std::string value) {
			submissions.emplace_back(std::move(value));
		});

	entry.submit("/help");

	ASSERT_EQ(submissions.size(), 1u);
	EXPECT_NE(submissions.front().find("/connect"), std::string::npos);
}

TEST(ConsoleEntryTest, ResignedSubmissionContractStopsReceivingLocalOutput)
{
	ConsoleEntry entry("Entry");
	std::size_t submissionCount = 0;
	auto contract = entry.subscribeToSubmission(
		[&](std::string) {
			++submissionCount;
		});

	entry.submit("/help");
	ASSERT_EQ(submissionCount, 1u);

	contract.resign();
	entry.submit("/help");

	EXPECT_EQ(submissionCount, 1u);
}
