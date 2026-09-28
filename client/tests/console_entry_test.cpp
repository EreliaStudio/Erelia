#include "erelia/client/command/connect_command.hpp"
#include "erelia/client/console_entry.hpp"
#include "erelia/client/service.hpp"

#include <diagnostics/logger.hpp>
#include <system/translator.hpp>

#include <gtest/gtest.h>

#include <optional>
#include <string>
#include <utility>
#include <vector>

namespace
{
	void registerConnectCommand(ConsoleEntry &entry)
	{
		entry.commandParser().addCommand<ConnectCommand>();
	}

	void installConsoleEntryTranslations()
	{
		Service::translator()->append("client.console.placeholder", "Enter text or /help");
		Service::translator()->append("client.console.command.unknown", "Unknown command: /{}");
		Service::translator()->append("client.console.command.invalid_format", "Invalid command format.");
		Service::translator()->append("client.console.command.parameter_unknown", "Unknown parameter: --{}");
		Service::translator()->append("client.console.command.parameter_duplicate", "Duplicate parameter: --{}");
		Service::translator()->append("client.console.command.parameter_missing", "Missing parameter: --{}");
		Service::translator()->append("client.console.command.value_missing", "Missing value for --{} (expected {}).");
		Service::translator()->append("client.console.command.too_many_values", "Too many values for --{} (expected {}, received {}).");
		Service::translator()->append("client.console.command.too_many_parameters", "Too many parameters.");
		Service::translator()->append("client.command.connect.description", "Starts a new dedicated Server connection cycle.");
		Service::translator()->append("client.command.connect.address.description", "Dedicated Server address");
		Service::translator()->append("client.command.connect.port.description", "Dedicated Server port");
		Service::translator()->append("client.command.connect.invalid_port", "Invalid port: {}");
	}

	class ConsoleEntryTest : public testing::Test
	{
	protected:
		void SetUp() override
		{
			Service::translator()->clear();
			installConsoleEntryTranslations();
		}

		void TearDown() override
		{
			Service::translator()->clear();
		}
	};
}

TEST_F(ConsoleEntryTest, OrdinaryTextUsesLogger)
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
	registerConnectCommand(entry);
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});

	entry.submit("hello");

	EXPECT_EQ(level, spk::Logger::Level::UserValueA);
	EXPECT_EQ(message, "hello");
	EXPECT_TRUE(local.empty());
}

TEST_F(ConsoleEntryTest, GlobalHelpIsLocalAndDoesNotUseLogger)
{
	std::vector<std::string> local;
	std::size_t loggerCalls = 0;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &, const std::string &) {
			++loggerCalls;
		});
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
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

TEST_F(ConsoleEntryTest, CommandHelpIsLocalAndDoesNotEmitConnectRequest)
{
	std::vector<std::string> local;
	std::size_t requestCount = 0;
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});
	auto contract = entry.commandParser().command<ConnectCommand>().subscribeToRequest(
		[&](const ConnectCommand::Request &) {
			++requestCount;
		});

	entry.submit("/connect --help");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_NE(local.front().find("Usage: /connect"), std::string::npos);
	EXPECT_EQ(requestCount, 0u);
}

TEST_F(ConsoleEntryTest, MalformedKnownCommandAddsDiagnosticAndUsageLocally)
{
	std::vector<std::string> local;
	std::size_t loggerCalls = 0;
	std::size_t requestCount = 0;
	auto loggerContract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &, const std::string &) {
			++loggerCalls;
		});
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});
	auto requestContract = entry.commandParser().command<ConnectCommand>().subscribeToRequest(
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

TEST_F(ConsoleEntryTest, UnknownCommandReportsLocallyWithoutLogger)
{
	std::vector<std::string> local;
	std::size_t loggerCalls = 0;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &, const std::string &) {
			++loggerCalls;
		});
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
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

TEST_F(ConsoleEntryTest, ConnectParametersAreOptionalAndShownInHelp)
{
	std::vector<std::string> local;
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto submissionContract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});

	entry.submit("/connect --help");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_NE(local.front().find("--address"), std::string::npos);
	EXPECT_NE(local.front().find("--port"), std::string::npos);
}

TEST_F(ConsoleEntryTest, ConnectWithoutOverridesEmitsEmptyRequest)
{
	std::optional<ConnectCommand::Request> received;
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto contract = entry.commandParser().command<ConnectCommand>().subscribeToRequest(
		[&](const ConnectCommand::Request &request) {
			received = request;
		});

	entry.submit("/connect");

	ASSERT_TRUE(received.has_value());
	EXPECT_FALSE(received->address.has_value());
	EXPECT_FALSE(received->port.has_value());
}

TEST_F(ConsoleEntryTest, ConnectAddressOverrideEmitsAddressOnly)
{
	std::optional<ConnectCommand::Request> received;
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto contract = entry.commandParser().command<ConnectCommand>().subscribeToRequest(
		[&](const ConnectCommand::Request &request) {
			received = request;
		});

	entry.submit("/connect --address 192.0.2.1");

	ASSERT_TRUE(received.has_value());
	ASSERT_TRUE(received->address.has_value());
	EXPECT_EQ(*received->address, "192.0.2.1");
	EXPECT_FALSE(received->port.has_value());
}

TEST_F(ConsoleEntryTest, ConnectPortOverrideEmitsPortOnly)
{
	std::optional<ConnectCommand::Request> received;
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto contract = entry.commandParser().command<ConnectCommand>().subscribeToRequest(
		[&](const ConnectCommand::Request &request) {
			received = request;
		});

	entry.submit("/connect --port 2550");

	ASSERT_TRUE(received.has_value());
	EXPECT_FALSE(received->address.has_value());
	ASSERT_TRUE(received->port.has_value());
	EXPECT_EQ(*received->port, 2550u);
}

TEST_F(ConsoleEntryTest, ConnectRejectsInvalidPortThroughCommandLogger)
{
	std::size_t requestCount = 0;
	spk::Logger::Level level = spk::Logger::Level::Trace;
	std::string message;
	auto loggerContract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &receivedLevel, const std::string &receivedMessage) {
			level = receivedLevel;
			message = receivedMessage;
		});
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto requestContract = entry.commandParser().command<ConnectCommand>().subscribeToRequest(
		[&](const ConnectCommand::Request &) {
			++requestCount;
		});

	entry.submit("/connect --port invalid");

	EXPECT_EQ(level, spk::Logger::Level::UserValueB);
	EXPECT_EQ(message, "Invalid port: invalid");
	EXPECT_EQ(requestCount, 0u);
}

TEST_F(ConsoleEntryTest, ConnectRejectsOutOfRangePortThroughCommandLogger)
{
	std::size_t requestCount = 0;
	spk::Logger::Level level = spk::Logger::Level::Trace;
	std::string message;
	auto loggerContract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &receivedLevel, const std::string &receivedMessage) {
			level = receivedLevel;
			message = receivedMessage;
		});
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto requestContract = entry.commandParser().command<ConnectCommand>().subscribeToRequest(
		[&](const ConnectCommand::Request &) {
			++requestCount;
		});

	entry.submit("/connect --port 65536");

	EXPECT_EQ(level, spk::Logger::Level::UserValueB);
	EXPECT_EQ(message, "Invalid port: 65536");
	EXPECT_EQ(requestCount, 0u);
}


TEST_F(ConsoleEntryTest, SubmissionContractReceivesLocalOutput)
{
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	std::vector<std::string> submissions;
	auto contract = entry.subscribeToSubmission(
		[&](std::string value) {
			submissions.emplace_back(std::move(value));
		});

	entry.submit("/help");

	ASSERT_EQ(submissions.size(), 1u);
	EXPECT_NE(submissions.front().find("/connect"), std::string::npos);
}

TEST_F(ConsoleEntryTest, ResignedSubmissionContractStopsReceivingLocalOutput)
{
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
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


TEST_F(ConsoleEntryTest, UsesTranslatedCommandFailure)
{
	Service::translator()->clear();
	Service::translator()->append("client.console.placeholder", "Commande");
	Service::translator()->append("client.console.command.unknown", "Commande inconnue : /{}");
	Service::translator()->append("client.command.connect.description", "Connexion");
	Service::translator()->append("client.command.connect.address.description", "Adresse");
	Service::translator()->append("client.command.connect.port.description", "Port");

	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	std::vector<std::string> local;
	auto contract = entry.subscribeToSubmission(
		[&](std::string value) {
			local.emplace_back(std::move(value));
		});

	entry.submit("/missing");

	ASSERT_EQ(local.size(), 1u);
	EXPECT_EQ(local.front(), "Commande inconnue : /missing");
}

TEST_F(ConsoleEntryTest, UsesTranslatedConnectValidationThroughCommandLogger)
{
	Service::translator()->clear();
	Service::translator()->append("client.console.placeholder", "Commande");
	Service::translator()->append("client.command.connect.description", "Connexion");
	Service::translator()->append("client.command.connect.address.description", "Adresse");
	Service::translator()->append("client.command.connect.port.description", "Port");
	Service::translator()->append("client.command.connect.invalid_port", "Port invalide : {}");

	spk::Logger::Level level = spk::Logger::Level::Trace;
	std::string message;
	auto loggerContract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &receivedLevel, const std::string &receivedMessage) {
			level = receivedLevel;
			message = receivedMessage;
		});
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);

	entry.submit("/connect --port invalid");

	EXPECT_EQ(level, spk::Logger::Level::UserValueB);
	EXPECT_EQ(message, "Port invalide : invalid");
}


TEST_F(ConsoleEntryTest, DoesNotRegisterConcreteCommands)
{
	ConsoleEntry entry("Entry");

	EXPECT_THROW(
		(void)entry.commandParser().command<ConnectCommand>(),
		spk::Exception);
}
