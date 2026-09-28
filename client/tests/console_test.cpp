#include "erelia/client/command/connect_command.hpp"
#include "erelia/client/console.hpp"
#include "erelia/client/service.hpp"

#include <core/context/update_context.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <input/device_context.hpp>
#include <system/translator.hpp>

#include <gtest/gtest.h>

#include <chrono>
#include <optional>
#include <string>

namespace
{
	void registerConnectCommand(Console &console)
	{
		console.commandParser().addCommand<ConnectCommand>();
	}

	void installConsoleTranslations()
	{
		Service::translator()->append("client.console.placeholder", "Enter text or /help");
		Service::translator()->append("client.console.user_label", "User");
		Service::translator()->append("client.console.command_label", "Command");
		Service::translator()->append("client.command.connect.description", "Starts a new dedicated Server connection cycle.");
		Service::translator()->append("client.command.connect.address.description", "Dedicated Server address");
		Service::translator()->append("client.command.connect.port.description", "Dedicated Server port");
		Service::translator()->append("client.command.connect.invalid_port", "Invalid port: {}");
	}

	void advance(spk::Widget &widget)
	{
		spk::UpdateContext context{.time = {}, .deltaTime = std::chrono::milliseconds(16)};
		spk::Keyboard keyboard;
		spk::Mouse mouse;
		spk::DeviceContext devices{.keyboard = keyboard, .mouse = mouse};
		widget.updateState(context);
		widget.updateState(context, devices);
	}

	class ConsoleTest : public testing::Test
	{
	protected:
		void SetUp() override
		{
			Service::translator()->clear();
			installConsoleTranslations();
		}

		void TearDown() override
		{
			Service::translator()->clear();
		}
	};
}

TEST_F(ConsoleTest, LoggerEntriesAreStoredAsIndependentModelRows)
{
	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

	SPK_LOG(UserValueA) << "hello" << std::endl;
	SPK_LOG(UserValueB) << "done" << std::endl;
	advance(console);

	ASSERT_EQ(console.entries().rowCount(), 2u);
	EXPECT_EQ(console.entries().data(0), "User : hello");
	EXPECT_EQ(console.entries().data(1), "[Command] : done");
}

TEST_F(ConsoleTest, MultilineLoggerEntryIsSplitIntoRowsWithoutTrailingEmptyRow)
{
	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

	SPK_LOG(UserValueB)
		<< "first\nsecond\nthird\n"
		<< std::endl;
	advance(console);

	ASSERT_EQ(console.entries().rowCount(), 3u);
	EXPECT_EQ(console.entries().data(0), "[Command] : first");
	EXPECT_EQ(console.entries().data(1), "[Command] : second");
	EXPECT_EQ(console.entries().data(2), "[Command] : third");
}

TEST_F(ConsoleTest, TenThousandLoggerEntriesArePreservedAndReachable)
{
	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

	for (std::size_t index = 0; index < 10'000; ++index)
	{
		SPK_LOG(UserValueA) << "line-" << index << std::endl;
	}
	advance(console);

	ASSERT_EQ(console.entries().rowCount(), 10'000u);
	for (std::size_t index = 0; index < 10'000; ++index)
	{
		EXPECT_EQ(console.entries().data(index), "User : line-" + std::to_string(index));
	}

	EXPECT_TRUE(console.entryView().isLastRowVisible());
	console.entryView().scrollTo(5'000);
	EXPECT_TRUE(console.entryView().isRowVisible(5'000));
	console.entryView().scrollTo(0);
	EXPECT_TRUE(console.entryView().isRowVisible(0));
	EXPECT_EQ(console.entries().data(9'999), "User : line-9999");
}

TEST_F(ConsoleTest, NewEntriesFollowTailOnlyWhileTailIsVisible)
{
	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 200}});

	for (std::size_t index = 0; index < 40; ++index)
	{
		SPK_LOG(UserValueA) << "initial-" << index << std::endl;
	}
	advance(console);
	ASSERT_TRUE(console.entryView().isLastRowVisible());

	SPK_LOG(UserValueA) << "following-tail" << std::endl;
	advance(console);
	EXPECT_TRUE(console.entryView().isLastRowVisible());

	console.entryView().scrollTo(0);
	ASSERT_FALSE(console.entryView().isLastRowVisible());
	SPK_LOG(UserValueA) << "while-reading-history" << std::endl;
	advance(console);
	EXPECT_TRUE(console.entryView().isRowVisible(0));
	EXPECT_FALSE(console.entryView().isLastRowVisible());
	EXPECT_EQ(console.entries().data(console.entries().rowCount() - 1), "User : while-reading-history");

	console.entryView().scrollTo(console.entries().rowCount() - 1);
	ASSERT_TRUE(console.entryView().isLastRowVisible());
	SPK_LOG(UserValueA) << "following-restored" << std::endl;
	advance(console);
	EXPECT_TRUE(console.entryView().isLastRowVisible());
	EXPECT_EQ(console.entries().data(console.entries().rowCount() - 1), "User : following-restored");
}

TEST_F(ConsoleTest, CommandHelpReachesModelThroughLogger)
{
	Console console("Console");
	registerConnectCommand(console);
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

	console.submit("/connect --help");
	EXPECT_EQ(console.entries().rowCount(), 0u);

	advance(console);

	ASSERT_EQ(console.entries().rowCount(), 4u);
	EXPECT_EQ(
		console.entries().data(0),
		"[Command] : Usage: /connect [--address <1 value>] [--port <1 value>]");
	EXPECT_EQ(
		console.entries().data(1),
		"[Command] : Starts a new dedicated Server connection cycle.");
	EXPECT_EQ(
		console.entries().data(2),
		"[Command] :   --address: Dedicated Server address");
	EXPECT_EQ(
		console.entries().data(3),
		"[Command] :   --port: Dedicated Server port");
}

TEST_F(ConsoleTest, GlobalHelpReachesModelWithoutTrailingEmptyRow)
{
	Console console("Console");
	registerConnectCommand(console);
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

	console.submit("/help");
	advance(console);

	ASSERT_EQ(console.entries().rowCount(), 1u);
	EXPECT_EQ(
		console.entries().data(0),
		"[Command] : /connect - Starts a new dedicated Server connection cycle.");
}

TEST_F(ConsoleTest, CommandParserExposesExplicitlyRegisteredConnectCommand)
{
	std::optional<ConnectCommand::Request> received;
	Console console("Console");
	registerConnectCommand(console);
	auto contract =
		console.commandParser()
			.command<ConnectCommand>()
			.subscribeToRequest(
				[&](const ConnectCommand::Request &request) {
					received = request;
				});

	console.submit("/connect --address 192.0.2.1 --port 2550");

	ASSERT_TRUE(received.has_value());
	ASSERT_TRUE(received->address.has_value());
	ASSERT_TRUE(received->port.has_value());
	EXPECT_EQ(*received->address, "192.0.2.1");
	EXPECT_EQ(*received->port, 2550u);
}

TEST_F(ConsoleTest, OnlyUserDataLoggerLevelsReachDataModel)
{
	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

	SPK_LOG(Trace) << "trace" << std::endl;
	SPK_LOG(Info) << "info" << std::endl;
	SPK_LOG(UserValueA) << "user" << std::endl;
	SPK_LOG(UserValueB) << "command" << std::endl;
	SPK_LOG(Warning) << "warning" << std::endl;
	SPK_LOG(Error) << "error" << std::endl;
	advance(console);

	ASSERT_EQ(console.entries().rowCount(), 2u);
	EXPECT_EQ(console.entries().data(0), "User : user");
	EXPECT_EQ(console.entries().data(1), "[Command] : command");
}

TEST_F(ConsoleTest, UsesTranslatedUserDataLabels)
{
	Service::translator()->clear();
	Service::translator()->append("client.console.placeholder", "Commande");
	Service::translator()->append("client.console.user_label", "Joueur");
	Service::translator()->append("client.console.command_label", "Commande");

	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

	SPK_LOG(UserValueA) << "bonjour" << std::endl;
	SPK_LOG(UserValueB) << "connect" << std::endl;
	advance(console);

	ASSERT_EQ(console.entries().rowCount(), 2u);
	EXPECT_EQ(console.entries().data(0), "Joueur : bonjour");
	EXPECT_EQ(console.entries().data(1), "[Commande] : connect");
}

TEST_F(ConsoleTest, ConnectValidationReachesConsoleThroughUserValueB)
{
	Console console("Console");
	registerConnectCommand(console);
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

	console.submit("/connect --port invalid");
	advance(console);

	ASSERT_EQ(console.entries().rowCount(), 1u);
	EXPECT_EQ(
		console.entries().data(0),
		"[Command] : Invalid port: invalid");
}

TEST_F(ConsoleTest, DoesNotOwnConcreteCommandsAndUsesHierarchicalChildNames)
{
	Console console("/Client/Console");

	EXPECT_EQ(console.name(), "/Client/Console");
	EXPECT_EQ(console.entryView().name(), "/Client/Console/entries");
	EXPECT_EQ(console.commandEntry().name(), "/Client/Console/command");
	EXPECT_THROW(
		(void)console.commandParser().command<ConnectCommand>(),
		spk::Exception);
}
