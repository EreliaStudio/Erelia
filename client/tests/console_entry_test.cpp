#include "erelia/client/command/connect_command.hpp"
#include "erelia/client/console_entry.hpp"
#include "erelia/client/service.hpp"

#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <system/translator.hpp>

#include <gtest/gtest.h>

#include <optional>
#include <string>
#include <utility>
#include <vector>

namespace
{
	using LogEntry = std::pair<spk::Logger::Level, std::string>;

	void registerConnectCommand(ConsoleEntry &entry)
	{
		entry.commandParser().addCommand<ConnectCommand>();
	}

	void installConsoleEntryTranslations()
	{
		Service::translator().append("client.console.placeholder", "Enter text or /help");
		Service::translator().append("client.console.command.unknown", "Unknown command: /{}");
		Service::translator().append("client.console.command.invalid_format", "Invalid command format.");
		Service::translator().append("client.console.command.parameter_unknown", "Unknown parameter: --{}");
		Service::translator().append("client.console.command.parameter_duplicate", "Duplicate parameter: --{}");
		Service::translator().append("client.console.command.parameter_missing", "Missing parameter: --{}");
		Service::translator().append("client.console.command.value_missing", "Missing value for --{} (expected {}).");
		Service::translator().append("client.console.command.too_many_values", "Too many values for --{} (expected {}, received {}).");
		Service::translator().append("client.console.command.too_many_parameters", "Too many parameters.");
		Service::translator().append("client.command.connect.description", "Starts a new dedicated Server connection cycle.");
		Service::translator().append("client.command.connect.address.description", "Dedicated Server address");
		Service::translator().append("client.command.connect.port.description", "Dedicated Server port");
		Service::translator().append("client.command.connect.invalid_port", "Invalid port: {}");
	}

	class ConsoleEntryTest : public testing::Test
	{
	protected:
		void SetUp() override
		{
			Service::translator().clear();
			installConsoleEntryTranslations();
		}

		void TearDown() override
		{
			Service::translator().clear();
		}
	};
}

TEST_F(ConsoleEntryTest, OrdinaryTextUsesUserValueA)
{
	std::vector<LogEntry> entries;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &level, const std::string &message) {
			entries.emplace_back(level, message);
		});
	ConsoleEntry entry("Entry");

	entry.submit("hello");

	ASSERT_EQ(entries.size(), 1u);
	EXPECT_EQ(entries[0].first, spk::Logger::Level::UserValueA);
	EXPECT_EQ(entries[0].second, "hello");
}

TEST_F(ConsoleEntryTest, GlobalHelpUsesUserValueB)
{
	std::vector<LogEntry> entries;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &level, const std::string &message) {
			entries.emplace_back(level, message);
		});
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);

	entry.submit("/help");

	ASSERT_EQ(entries.size(), 1u);
	EXPECT_EQ(entries[0].first, spk::Logger::Level::UserValueB);
	EXPECT_NE(entries[0].second.find("/connect"), std::string::npos);
}

TEST_F(ConsoleEntryTest, CommandHelpUsesUserValueBWithoutRequest)
{
	std::vector<LogEntry> entries;
	std::size_t requestCount = 0;
	auto loggerContract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &level, const std::string &message) {
			entries.emplace_back(level, message);
		});
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto requestContract =
		Service::clientEventCenter().connectionRequested().subscribe(
			[&](const Client::ConnectionRequest &) {
				++requestCount;
			});

	entry.submit("/connect --help");

	ASSERT_EQ(entries.size(), 1u);
	EXPECT_EQ(entries[0].first, spk::Logger::Level::UserValueB);
	EXPECT_NE(entries[0].second.find("Usage: /connect"), std::string::npos);
	EXPECT_EQ(requestCount, 0u);
}

TEST_F(ConsoleEntryTest, MalformedKnownCommandLogsDiagnosticAndUsage)
{
	std::vector<LogEntry> entries;
	std::size_t requestCount = 0;
	auto loggerContract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &level, const std::string &message) {
			entries.emplace_back(level, message);
		});
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto requestContract =
		Service::clientEventCenter().connectionRequested().subscribe(
			[&](const Client::ConnectionRequest &) {
				++requestCount;
			});

	entry.submit("/connect --unknown value");

	ASSERT_EQ(entries.size(), 2u);
	EXPECT_EQ(entries[0].first, spk::Logger::Level::UserValueB);
	EXPECT_EQ(entries[0].second, "Unknown parameter: --unknown");
	EXPECT_EQ(entries[1].first, spk::Logger::Level::UserValueB);
	EXPECT_NE(entries[1].second.find("Usage: /connect"), std::string::npos);
	EXPECT_EQ(requestCount, 0u);
}

TEST_F(ConsoleEntryTest, UnknownCommandUsesUserValueB)
{
	std::vector<LogEntry> entries;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &level, const std::string &message) {
			entries.emplace_back(level, message);
		});
	ConsoleEntry entry("Entry");

	entry.submit("/missing");

	ASSERT_EQ(entries.size(), 1u);
	EXPECT_EQ(entries[0].first, spk::Logger::Level::UserValueB);
	EXPECT_EQ(entries[0].second, "Unknown command: /missing");
}

TEST_F(ConsoleEntryTest, ConnectWithoutOverridesEmitsEmptyRequest)
{
	std::optional<Client::ConnectionRequest> received;
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto contract =
		Service::clientEventCenter().connectionRequested().subscribe(
			[&](const Client::ConnectionRequest &request) {
				received = request;
			});

	entry.submit("/connect");

	ASSERT_TRUE(received.has_value());
	EXPECT_FALSE(received->address.has_value());
	EXPECT_FALSE(received->port.has_value());
}

TEST_F(ConsoleEntryTest, ConnectAddressOverrideEmitsAddressOnly)
{
	std::optional<Client::ConnectionRequest> received;
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto contract =
		Service::clientEventCenter().connectionRequested().subscribe(
			[&](const Client::ConnectionRequest &request) {
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
	std::optional<Client::ConnectionRequest> received;
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto contract =
		Service::clientEventCenter().connectionRequested().subscribe(
			[&](const Client::ConnectionRequest &request) {
				received = request;
			});

	entry.submit("/connect --port 2550");

	ASSERT_TRUE(received.has_value());
	EXPECT_FALSE(received->address.has_value());
	ASSERT_TRUE(received->port.has_value());
	EXPECT_EQ(*received->port, 2550u);
}

TEST_F(ConsoleEntryTest, ConnectRejectsInvalidPortThroughUserValueB)
{
	std::vector<LogEntry> entries;
	std::size_t requestCount = 0;
	auto loggerContract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &level, const std::string &message) {
			entries.emplace_back(level, message);
		});
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto requestContract =
		Service::clientEventCenter().connectionRequested().subscribe(
			[&](const Client::ConnectionRequest &) {
				++requestCount;
			});

	entry.submit("/connect --port invalid");

	ASSERT_EQ(entries.size(), 1u);
	EXPECT_EQ(entries[0].first, spk::Logger::Level::UserValueB);
	EXPECT_EQ(entries[0].second, "Invalid port: invalid");
	EXPECT_EQ(requestCount, 0u);
}

TEST_F(ConsoleEntryTest, ConnectRejectsOutOfRangePortThroughUserValueB)
{
	std::vector<LogEntry> entries;
	std::size_t requestCount = 0;
	auto loggerContract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &level, const std::string &message) {
			entries.emplace_back(level, message);
		});
	ConsoleEntry entry("Entry");
	registerConnectCommand(entry);
	auto requestContract =
		Service::clientEventCenter().connectionRequested().subscribe(
			[&](const Client::ConnectionRequest &) {
				++requestCount;
			});

	entry.submit("/connect --port 65536");

	ASSERT_EQ(entries.size(), 1u);
	EXPECT_EQ(entries[0].first, spk::Logger::Level::UserValueB);
	EXPECT_EQ(entries[0].second, "Invalid port: 65536");
	EXPECT_EQ(requestCount, 0u);
}

TEST_F(ConsoleEntryTest, UsesTranslatedParserFailureThroughUserValueB)
{
	Service::translator().clear();
	Service::translator().append("client.console.placeholder", "Commande");
	Service::translator().append("client.console.command.unknown", "Commande inconnue : /{}");

	std::vector<LogEntry> entries;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &level, const std::string &message) {
			entries.emplace_back(level, message);
		});
	ConsoleEntry entry("Entry");

	entry.submit("/missing");

	ASSERT_EQ(entries.size(), 1u);
	EXPECT_EQ(entries[0].first, spk::Logger::Level::UserValueB);
	EXPECT_EQ(entries[0].second, "Commande inconnue : /missing");
}

TEST_F(ConsoleEntryTest, DoesNotRegisterConcreteCommands)
{
	ConsoleEntry entry("Entry");

	EXPECT_THROW(
		(void)entry.commandParser().command<ConnectCommand>(),
		spk::Exception);
}
