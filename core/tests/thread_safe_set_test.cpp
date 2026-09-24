#include <container/thread_safe_set.hpp>

#include <gtest/gtest.h>

#include <cstddef>
#include <thread>
#include <unordered_set>
#include <vector>

TEST(ThreadSafeSet, ProducerConsumerEndpointsDeduplicateConcurrentPublish)
{
	auto endpoints = spk::ThreadSafeSet<int>::create();
	auto secondProducer = endpoints.producer;

	std::vector<std::thread> threads;
	for (std::size_t index = 0; index < 8u; ++index)
	{
		threads.emplace_back(
			[producer = index % 2u == 0u
							? endpoints.producer
							: secondProducer]() mutable {
				for (int repetition = 0; repetition < 100; ++repetition)
				{
					(void)producer.publish(7);
					(void)producer.publish(11);
				}
			});
	}

	for (std::thread &thread : threads)
	{
		thread.join();
	}

	const auto &values = endpoints.consumer.drain();
	EXPECT_EQ(values.size(), 2u);
	EXPECT_TRUE(values.contains(7));
	EXPECT_TRUE(values.contains(11));
	EXPECT_TRUE(endpoints.consumer.drain().empty());
}

TEST(ThreadSafeSet, PublishReportsWhetherValueWasNew)
{
	spk::ThreadSafeSet<int> values;

	EXPECT_TRUE(values.publish(4));
	EXPECT_FALSE(values.publish(4));
	EXPECT_TRUE(values.emplace(8));
	EXPECT_FALSE(values.emplace(8));
	EXPECT_TRUE(values.contains(4));
	EXPECT_TRUE(values.erase(4));
	EXPECT_FALSE(values.contains(4));
	EXPECT_FALSE(values.erase(4));
}
