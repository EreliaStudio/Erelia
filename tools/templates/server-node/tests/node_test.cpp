#include "__NODE_SNAKE___node.hpp"
#include "__NODE_SNAKE___node_application.hpp"

#include <gtest/gtest.h>

#include <chrono>
#include <csignal>
#include <exception>
#include <thread>

using namespace std::chrono_literals;

namespace
{
	template <typename TPredicate>
	[[nodiscard]] bool waitUntil(
		TPredicate predicate,
		std::chrono::milliseconds timeout = 2s)
	{
		const auto deadline =
			std::chrono::steady_clock::now() + timeout;
		while (std::chrono::steady_clock::now() < deadline)
		{
			if (predicate())
			{
				return true;
			}
			std::this_thread::sleep_for(5ms);
		}
		return predicate();
	}

	void expectApplicationStopsOnSignal(int signal)
	{
		__NODE_NAME__NodeApplication application(
			__NODE_NAME__Node::Configuration{
				.port = 0});

		std::exception_ptr applicationFailure;
		std::thread applicationThread(
			[&]()
			{
				try
				{
					application.run();
				} catch (...)
				{
					applicationFailure =
						std::current_exception();
				}
			});

		if (!waitUntil(
				[&]()
				{
					return application.isRunning();
				}))
		{
			application.stop();
			applicationThread.join();
			ADD_FAILURE()
				<< "Node application did not start before the deadline";
			return;
		}

		if (std::raise(signal) != 0)
		{
			application.stop();
			applicationThread.join();
			ADD_FAILURE()
				<< "Unable to raise signal "
				<< signal;
			return;
		}

		if (!waitUntil(
				[&]()
				{
					return application.isRunning() == false;
				}))
		{
			application.stop();
			applicationThread.join();
			ADD_FAILURE()
				<< "Node application did not stop after signal "
				<< signal;
			return;
		}

		applicationThread.join();
		EXPECT_FALSE(applicationFailure);
	}
}

TEST(__NODE_NAME__NodeRuntime, StartsStopsAndRestarts)
{
	__NODE_NAME__Node node(
		__NODE_NAME__Node::Configuration{
			.port = 0});

	node.start();
	EXPECT_TRUE(node.isRunning());
	EXPECT_NE(node.port(), 0u);

	node.stop();
	EXPECT_FALSE(node.isRunning());

	node.start();
	EXPECT_TRUE(node.isRunning());
	node.stop();
}

TEST(__NODE_NAME__NodeApplication, StopsCleanlyOnInterruptSignal)
{
	expectApplicationStopsOnSignal(SIGINT);
}

TEST(__NODE_NAME__NodeApplication, StopsCleanlyOnTerminationSignal)
{
	expectApplicationStopsOnSignal(SIGTERM);
}
