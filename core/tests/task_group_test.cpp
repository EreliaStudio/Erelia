#include "erelia/core/task_group.hpp"

#include <gtest/gtest.h>

#include <chrono>
#include <future>
#include <stdexcept>
#include <thread>
#include <utility>

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
			std::this_thread::yield();
		}

		return predicate();
	}
}

TEST(TaskGroup, EmptyGroupCompletesImmediately)
{
	spk::WorkerPool workerPool(1u);
	spk::TaskGroup<int> group;

	auto answer = std::move(group).submit(workerPool);

	EXPECT_EQ(answer.status(), spk::Task<int>::Status::Completed);
	EXPECT_EQ(answer.size(), 0u);
	EXPECT_TRUE(answer.answers().empty());
}

TEST(TaskGroup, RemainsPendingUntilEveryTaskSettles)
{
	spk::WorkerPool workerPool(2u);
	std::promise<void> firstRelease;
	std::promise<void> secondRelease;
	const std::shared_future<void> firstGate =
		firstRelease.get_future().share();
	const std::shared_future<void> secondGate =
		secondRelease.get_future().share();

	spk::TaskGroup<int> group;
	group.add(
		spk::Task<int>(
			[firstGate] {
				firstGate.wait();
				return 10;
			}));
	group.add(
		spk::Task<int>(
			[secondGate] {
				secondGate.wait();
				return 20;
			}));

	auto answer = std::move(group).submit(workerPool);
	ASSERT_EQ(answer.size(), 2u);
	EXPECT_EQ(answer.status(), spk::Task<int>::Status::Pending);

	firstRelease.set_value();
	ASSERT_TRUE(
		waitUntil(
			[&] {
				return answer.at(0u).status() !=
					   spk::Task<int>::Status::Pending;
			}));

	EXPECT_EQ(answer.at(0u).status(), spk::Task<int>::Status::Completed);
	EXPECT_EQ(answer.at(0u).result(), 10);
	EXPECT_EQ(answer.status(), spk::Task<int>::Status::Pending);

	secondRelease.set_value();
	ASSERT_TRUE(
		waitUntil(
			[&] {
				return answer.status() !=
					   spk::Task<int>::Status::Pending;
			}));

	EXPECT_EQ(answer.status(), spk::Task<int>::Status::Completed);
	EXPECT_EQ(answer.at(1u).status(), spk::Task<int>::Status::Completed);
	EXPECT_EQ(answer.at(1u).result(), 20);
}

TEST(TaskGroup, FailureIsReportedOnlyAfterEveryTaskSettles)
{
	spk::WorkerPool workerPool(2u);
	std::promise<void> release;
	const std::shared_future<void> gate =
		release.get_future().share();

	spk::TaskGroup<int> group;
	group.add(
		spk::Task<int>(
			[]() -> int {
				throw std::runtime_error("group failure");
			}));
	group.add(
		spk::Task<int>(
			[gate] {
				gate.wait();
				return 42;
			}));

	auto answer = std::move(group).submit(workerPool);

	ASSERT_TRUE(
		waitUntil(
			[&] {
				return answer.at(0u).status() ==
					   spk::Task<int>::Status::Failed;
			}));
	EXPECT_EQ(answer.status(), spk::Task<int>::Status::Pending);

	release.set_value();
	ASSERT_TRUE(
		waitUntil(
			[&] {
				return answer.status() !=
					   spk::Task<int>::Status::Pending;
			}));

	EXPECT_EQ(answer.status(), spk::Task<int>::Status::Failed);
	EXPECT_EQ(answer.at(1u).result(), 42);
	EXPECT_THROW(
		std::rethrow_exception(answer.at(0u).failure()),
		std::runtime_error);
}

TEST(TaskGroup, ChildAnswersPreserveInsertionOrder)
{
	spk::WorkerPool workerPool(3u);
	spk::TaskGroup<int> group;

	group.add(spk::Task<int>([] { return 11; }));
	group.add(spk::Task<int>([] { return 22; }));
	group.add(spk::Task<int>([] { return 33; }));

	auto answer = std::move(group).submit(workerPool);

	ASSERT_TRUE(
		waitUntil(
			[&] {
				return answer.status() !=
					   spk::Task<int>::Status::Pending;
			}));

	ASSERT_EQ(answer.status(), spk::Task<int>::Status::Completed);
	ASSERT_EQ(answer.size(), 3u);
	EXPECT_EQ(answer.at(0u).result(), 11);
	EXPECT_EQ(answer.at(1u).result(), 22);
	EXPECT_EQ(answer.at(2u).result(), 33);
	EXPECT_THROW((void)answer.at(3u), spk::Exception);
}
