#include "erelia/client/connection_manager.hpp"
#include "erelia/client/console.hpp"

#include <core/context/update_context.hpp>
#include <design_pattern/singleton.hpp>
#include <diagnostics/logger.hpp>
#include <input/device_context.hpp>
#include <network/client.hpp>
#include <threading/worker_pool.hpp>

#include <gtest/gtest.h>

#include <chrono>
#include <string>

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

	void advance(spk::Widget &widget)
	{
		spk::UpdateContext context{.time = {}, .deltaTime = std::chrono::milliseconds(16)};
		spk::Keyboard keyboard;
		spk::Mouse mouse;
		spk::DeviceContext devices{.keyboard = keyboard, .mouse = mouse};
		widget.updateState(context);
		widget.updateState(context, devices);
	}

	class ConsoleTest : public ::testing::Test
	{
	protected:
		static void SetUpTestSuite()
		{
			ensureClientServices();
		}
	};
}

TEST_F(ConsoleTest, LoggerEntriesAreStoredAsIndependentModelRows)
{
	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
	Console console("Console", manager);
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

	SPK_LOG(UserValueA) << "hello" << std::endl;
	SPK_LOG(UserValueB) << "done" << std::endl;
	advance(console);

	ASSERT_EQ(console.entries().rowCount(), 2u);
	EXPECT_EQ(console.entries().data(0), "User : hello");
	EXPECT_EQ(console.entries().data(1), "[Command] : done");
}

TEST_F(ConsoleTest, TenThousandLoggerEntriesArePreservedAndReachable)
{
	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
	Console console("Console", manager);
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
	ConnectionManager manager("ConnectionManager", {"127.0.0.1", 1});
	Console console("Console", manager);
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
