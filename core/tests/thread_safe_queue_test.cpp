#include <container/thread_safe_queue.hpp>

#include <gtest/gtest.h>

#include <algorithm>
#include <future>
#include <mutex>
#include <thread>
#include <vector>

TEST(ThreadSafeQueue, ProducerConsumerPreservesFifoOrder)
{
	auto endpoints = spk::ThreadSafeQueue<int>::create();

	endpoints.producer.publish(3);
	endpoints.producer.publish(7);
	endpoints.producer.emplace(11);

	const auto first = endpoints.consumer.waitPop();
	const auto second = endpoints.consumer.waitPop();
	const auto third = endpoints.consumer.waitPop();

	ASSERT_TRUE(first.has_value());
	ASSERT_TRUE(second.has_value());
	ASSERT_TRUE(third.has_value());
	EXPECT_EQ(*first, 3);
	EXPECT_EQ(*second, 7);
	EXPECT_EQ(*third, 11);
}

TEST(ThreadSafeQueue, MultipleConsumersPopEachValueExactlyOnce)
{
	spk::ThreadSafeQueue<int> queue;
	for (int value = 0; value < 100; ++value)
	{
		queue.publish(value);
	}

	auto firstConsumer = queue.consumer();
	auto secondConsumer = queue.consumer();

	std::mutex valuesMutex;
	std::vector<int> values;
	values.reserve(100u);

	auto consume = [&](auto consumer) mutable {
		for (int index = 0; index < 50; ++index)
		{
			const auto value = consumer.waitPop();
			ASSERT_TRUE(value.has_value());

			const std::scoped_lock lock(valuesMutex);
			values.push_back(*value);
		}
	};

	std::thread firstThread(
		consume,
		std::move(firstConsumer));
	std::thread secondThread(
		consume,
		std::move(secondConsumer));

	firstThread.join();
	secondThread.join();

	std::ranges::sort(values);
	ASSERT_EQ(values.size(), 100u);
	for (int index = 0; index < 100; ++index)
	{
		EXPECT_EQ(values[static_cast<std::size_t>(index)], index);
	}
}

TEST(ThreadSafeQueue, StopTokenUnblocksEmptyConsumer)
{
	auto endpoints = spk::ThreadSafeQueue<int>::create();
	std::promise<bool> stopped;
	auto result = stopped.get_future();

	std::jthread waiter(
		[consumer = std::move(endpoints.consumer),
		 &stopped](std::stop_token stopToken) mutable {
			stopped.set_value(
				!consumer.waitPop(stopToken).has_value());
		});

	waiter.request_stop();
	EXPECT_TRUE(result.get());
}
