#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/collection.hpp"
#include <atomic>
#include <barrier>
#include <chrono>
#include <gtest/gtest.h>
#include <thread>
namespace
{
	using Cache = Collection<int, int>;
	struct Controlled : Cache::Provider
	{
		std::shared_ptr<std::unordered_map<int, std::shared_ptr<spk::Task<int>>>> tasks;
		explicit Controlled(decltype(tasks) value) :
			tasks(std::move(value))
		{
		}
		Cache::Answer _acquire(const int &key) override
		{
			auto task = std::make_shared<spk::Task<int>>();
			tasks->insert_or_assign(key, task);
			return task->answer();
		}
	};
	struct Immediate : Cache::Provider
	{
		Cache::Answer _acquire(const int &key) override
		{
			spk::Task<int> task;
			task.validate(key * 2);
			return task.answer();
		}
	};
	struct Throwing : Cache::Provider
	{
		Cache::Answer _acquire(const int &) override
		{
			throw spk::Exception("Rejected");
		}
	};
	using Tasks = std::unordered_map<int, std::shared_ptr<spk::Task<int>>>;
	static_assert(std::is_constructible_v<Cache, Controlled>);
	static_assert(std::is_constructible_v<Cache, Controlled &> == false);
}
TEST(Collection, AbsentPendingAvailableAndAvailableImmediate)
{
	auto tasks = std::make_shared<Tasks>();
	Cache cache{Controlled(tasks)};
	EXPECT_EQ(cache.state(3), Cache::State::Absent);
	EXPECT_EQ(cache.tryRead(3, [](const int &) {
	}),
			  false);
	auto answer = cache.request(3);
	auto repeated = cache.request(3);
	EXPECT_EQ(cache.state(3), Cache::State::Pending);
	EXPECT_EQ(tasks->size(), 1u);
	tasks->at(3)->validate(7);
	EXPECT_EQ(answer.result(), 7);
	EXPECT_EQ(&answer.result(), &repeated.result());
	EXPECT_EQ(cache.state(3), Cache::State::Available);
	EXPECT_TRUE(cache.tryRead(3, [](const int &value) {
		EXPECT_EQ(value, 7);
	}));
	EXPECT_EQ(cache.request(3).result(), 7);
}
TEST(Collection, OrderedGroupReusesDuplicatePendingAndWaitsForEveryChild)
{
	auto tasks = std::make_shared<Tasks>();
	Cache cache{Controlled(tasks)};
	cache.insert(9, 90);
	auto group = cache.request(std::vector<int>{2, 9, 1, 2});
	EXPECT_EQ(tasks->size(), 2u);
	EXPECT_EQ(group.size(), 4u);
	tasks->at(1)->fail(std::make_exception_ptr(spk::Exception("Failed")));
	EXPECT_EQ(group.status(), spk::Task<int>::Status::Pending);
	tasks->at(2)->validate(20);
	EXPECT_EQ(group.status(), spk::Task<int>::Status::Failed);
	EXPECT_EQ(group.at(0).result(), 20);
	EXPECT_EQ(group.at(1).result(), 90);
	EXPECT_EQ(group.at(2).status(), spk::Task<int>::Status::Failed);
	EXPECT_EQ(&group.at(0).result(), &group.at(3).result());
}
TEST(Collection, SynchronousCompletionAndEmptyGroup)
{
	Cache cache{Immediate{}};
	for (int key = 0; key < 50; ++key)
	{
		EXPECT_EQ(cache.request(key).result(), key * 2);
		EXPECT_EQ(cache.state(key), Cache::State::Available);
	}
	EXPECT_EQ(cache.request(std::vector<int>{}).status(), spk::Task<int>::Status::Completed);
}
TEST(Collection, StrictMutationsAndStaleCompletion)
{
	auto tasks = std::make_shared<Tasks>();
	Cache cache{Controlled(tasks)};
	EXPECT_THROW(cache.replace(1, 2), spk::Exception);
	cache.remove(1);
	auto pending = cache.request(1);
	auto stale = tasks->at(1);
	cache.insert(1, 20);
	EXPECT_EQ(pending.status(), spk::Task<int>::Status::Failed);
	stale->validate(10);
	EXPECT_EQ(cache.request(1).result(), 20);
	EXPECT_THROW(cache.insert(1, 30), spk::Exception);
	cache.replace(1, 30);
	EXPECT_EQ(cache.request(1).result(), 30);
	cache.remove(1);
	EXPECT_EQ(cache.state(1), Cache::State::Absent);
	pending = cache.request(1);
	stale = tasks->at(1);
	EXPECT_THROW(cache.replace(1, 40), spk::Exception);
	EXPECT_EQ(pending.status(), spk::Task<int>::Status::Failed);
	stale->validate(100);
	EXPECT_EQ(cache.state(1), Cache::State::Absent);
}
TEST(Collection, RemoveThenRerequestRejectsPreviousIdentity)
{
	auto tasks = std::make_shared<Tasks>();
	Cache cache{Controlled(tasks)};
	auto old = cache.request(1);
	auto stale = tasks->at(1);
	cache.remove(1);
	auto current = cache.request(1);
	stale->validate(3);
	EXPECT_EQ(current.status(), spk::Task<int>::Status::Pending);
	tasks->at(1)->validate(4);
	EXPECT_EQ(current.result(), 4);
	EXPECT_EQ(old.status(), spk::Task<int>::Status::Failed);
}
TEST(Collection, FailureLeavesUnrelatedAvailableAndCanRetry)
{
	Cache cache{Throwing{}};
	cache.insert(2, 7);
	EXPECT_EQ(cache.request(1).status(), spk::Task<int>::Status::Failed);
	EXPECT_EQ(cache.state(1), Cache::State::Absent);
	EXPECT_EQ(cache.request(2).result(), 7);
}
TEST(Collection, DestructionFailsPendingAndLateSourceIsSafe)
{
	auto tasks = std::make_shared<Tasks>();
	std::optional<Cache::Answer> answer;
	{
		Cache cache{Controlled(tasks)};
		answer = cache.request(1);
	}
	EXPECT_EQ(answer->status(), spk::Task<int>::Status::Failed);
	tasks->at(1)->validate(2);
}
TEST(Collection, ConcurrentRequestsAcquireOnceAndMutationCompletionRace)
{
	auto tasks = std::make_shared<Tasks>();
	Cache cache{Controlled(tasks)};
	// Register before racing request readers, so controlled fixture mutation is single-threaded.
	(void)cache.request(1);
	std::vector<std::jthread> readers;
	for (int index = 0; index < 16; ++index)
	{
		readers.emplace_back([&] {
			auto answer = cache.request(1);
			EXPECT_EQ(answer.status(), spk::Task<int>::Status::Pending);
		});
	}
	readers.clear();
	EXPECT_EQ(tasks->size(), 1u);
	std::barrier gate(2);
	std::jthread completion([&] {
		gate.arrive_and_wait();
		tasks->at(1)->validate(3);
	});
	gate.arrive_and_wait();
	cache.remove(1);
	completion.join();
	EXPECT_EQ(cache.state(1), Cache::State::Absent);
}
TEST(Collection, AuthoritativeInsertPublishesBeforeInvalidationCallbacks)
{
	auto tasks = std::make_shared<Tasks>();
	Cache cache{Controlled(tasks)};
	auto old = cache.request(1);
	std::optional<Cache::Answer> observed;
	auto subscription = old.subscribeToCompletion([&] {
		observed = cache.request(1);
	});
	cache.insert(1, 99);
	ASSERT_TRUE(observed.has_value());
	EXPECT_EQ(observed->result(), 99);
	EXPECT_EQ(tasks->size(), 1u);
}
TEST(Collection, RemoveAndRerequestInRegistrationLaunchWindowAcquiresReplacementOnce)
{
	struct Gate
	{
		std::atomic_bool first{true};
		std::atomic_bool entered{false};
		std::atomic_bool release{false};
		std::atomic_int acquisitions{0};
	};
	struct Delayed : Controlled
	{
		std::shared_ptr<Gate> gate;
		Delayed(std::shared_ptr<Tasks> tasks, std::shared_ptr<Gate> value) :
			Controlled(std::move(tasks)),
			gate(std::move(value))
		{
		}
		void _start(const std::vector<int> &keys) override
		{
			if (gate->first.exchange(false) == true)
			{
				gate->entered.store(true);
				gate->entered.notify_all();
				gate->release.wait(false);
			}
			Cache::Provider::_start(keys);
		}
		Cache::Answer _acquire(const int &key) override
		{
			++gate->acquisitions;
			return Controlled::_acquire(key);
		}
	};
	auto tasks = std::make_shared<Tasks>();
	auto gate = std::make_shared<Gate>();
	Cache cache{Delayed(tasks, gate)};
	std::optional<Cache::Answer> old;
	std::jthread first([&] {
		old = cache.request(1);
	});
	gate->entered.wait(false);
	cache.remove(1);
	auto current = cache.request(1);
	gate->release.store(true);
	gate->release.notify_all();
	first.join();
	EXPECT_EQ(gate->acquisitions.load(), 1);
	EXPECT_EQ(old->status(), spk::Task<int>::Status::Failed);
	tasks->at(1)->validate(99);
	EXPECT_EQ(current.result(), 99);
}
TEST(Collection, CompletionCallbacksDoNotHoldAcquisitionMutex)
{
	struct Prepared : Cache::Provider
	{
		std::shared_ptr<Tasks> tasks;
		explicit Prepared(std::shared_ptr<Tasks> value) :
			tasks(std::move(value))
		{
		}
		Cache::Answer _acquire(const int &key) override
		{
			return tasks->at(key)->answer();
		}
	};
	auto tasks = std::make_shared<Tasks>();
	tasks->emplace(1, std::make_shared<spk::Task<int>>());
	tasks->emplace(2, std::make_shared<spk::Task<int>>());
	std::atomic_bool entered{false}, release{false};
	auto sourceObserver = tasks->at(2)->answer().subscribeToCompletion([&] {
		entered.store(true);
		entered.notify_all();
	});
	Cache cache{Prepared(tasks)};
	auto answers = cache.request(std::vector<int>{1, 2});
	bool otherPublished = false;
	auto observer = answers.at(0).subscribeToCompletion([&] {
		release.store(true);
		release.notify_all();
		entered.wait(false);
		const auto deadline = std::chrono::steady_clock::now() + std::chrono::seconds(1);
		while (answers.at(1).status() == spk::Task<int>::Status::Pending && std::chrono::steady_clock::now() < deadline)
		{
			std::this_thread::yield();
		}
		otherPublished = answers.at(1).status() == spk::Task<int>::Status::Completed;
	});
	std::jthread second([&] {
		release.wait(false);
		tasks->at(2)->validate(2);
	});
	tasks->at(1)->validate(1);
	second.join();
	EXPECT_EQ(otherPublished, true);
	EXPECT_EQ(answers.at(1).result(), 2);
}
