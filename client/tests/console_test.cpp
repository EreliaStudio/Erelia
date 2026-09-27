#include "erelia/client/console.hpp"

#include <core/context/update_context.hpp>
#include <diagnostics/logger.hpp>
#include <input/device_context.hpp>

#include <gtest/gtest.h>

#include <chrono>
#include <optional>
#include <string>

namespace
{
	void advance(spk::Widget &widget)
	{
		spk::UpdateContext context{.time = {}, .deltaTime = std::chrono::milliseconds(16)};
		spk::Keyboard keyboard;
		spk::Mouse mouse;
		spk::DeviceContext devices{.keyboard = keyboard, .mouse = mouse};
		widget.updateState(context);
		widget.updateState(context, devices);
	}
}

TEST(ConsoleTest, LoggerEntriesAreStoredAsIndependentModelRows)
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

TEST(ConsoleTest, TenThousandLoggerEntriesArePreservedAndReachable)
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

TEST(ConsoleTest, NewEntriesFollowTailOnlyWhileTailIsVisible)
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

TEST(ConsoleTest, CommandHelpIsStoredAsIndependentModelRows)
{
	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

	console.submit("/connect --help");

	ASSERT_EQ(console.entries().rowCount(), 4u);
	EXPECT_EQ(
		console.entries().data(0),
		"Usage: /connect [--address <1 value>] [--port <1 value>]");
	EXPECT_EQ(console.entries().data(1), "Starts a new dedicated Server connection cycle.");
	EXPECT_EQ(console.entries().data(2), "  --address: Dedicated Server address");
	EXPECT_EQ(console.entries().data(3), "  --port: Dedicated Server port");
}

TEST(ConsoleTest, GlobalHelpDoesNotStoreTrailingEmptyModelRow)
{
	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

	console.submit("/help");

	ASSERT_EQ(console.entries().rowCount(), 1u);
	EXPECT_EQ(console.entries().data(0), "/connect - Starts a new dedicated Server connection cycle.");
}

TEST(ConsoleTest, ConnectRequestIsForwardedFromCommandEntry)
{
	std::optional<Console::ConnectRequest> received;
	Console console("Console");
	auto contract = console.subscribeToConnectRequest(
		[&](const Console::ConnectRequest &request) {
			received = request;
		});

	console.submit("/connect --address 192.0.2.1 --port 2550");

	ASSERT_TRUE(received.has_value());
	ASSERT_TRUE(received->address.has_value());
	ASSERT_TRUE(received->port.has_value());
	EXPECT_EQ(*received->address, "192.0.2.1");
	EXPECT_EQ(*received->port, 2550u);
}
