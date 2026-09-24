#include <threading/task.hpp>
#include <threading/worker_pool.hpp>

#include <exception.hpp>
#include <gtest/gtest.h>

#include <chrono>
#include <future>
#include <thread>

namespace
{
	template <typename TResult>
	bool waitUntilSettled(
		const typename spk::Task<TResult>::Answer &answer)
	{
		const auto deadline =
			std::chrono::steady_clock::now() +
			std::chrono::seconds(2);

		while (
			answer.status() == spk::Task<TResult>::Status::Pending &&
			std::chrono::steady_clock::now() < deadline)
		{
			std::this_thread::yield();
		}

		return answer.status() != spk::Task<TResult>::Status::Pending;
	}
}

TEST(TaskWorkerPool, AnswerTransitionsFromPendingToCompleted)
{
	spk::WorkerPool workerPool(1u);
	std::promise<void> release;
	std::shared_future<void> gate = release.get_future().share();

	spk::Task<int> task(
		[gate] {
			gate.wait();
			return 42;
		});

	auto answer = workerPool.submit(std::move(task));
	EXPECT_EQ(answer.status(), spk::Task<int>::Status::Pending);

	release.set_value();
	ASSERT_TRUE(waitUntilSettled<int>(answer));
	EXPECT_EQ(answer.status(), spk::Task<int>::Status::Completed);
	EXPECT_EQ(answer.result(), 42);
	EXPECT_THROW((void)answer.failure(), spk::Exception);
}

TEST(TaskWorkerPool, EscapingExceptionProducesFailedAnswer)
{
	spk::WorkerPool workerPool(1u);
	spk::Task<int> task(
		[]() -> int {
			throw spk::Exception("expected task failure");
		});

	auto answer = workerPool.submit(std::move(task));
	ASSERT_TRUE(waitUntilSettled<int>(answer));
	EXPECT_EQ(answer.status(), spk::Task<int>::Status::Failed);
	EXPECT_THROW((void)answer.result(), spk::Exception);

	try
	{
		std::rethrow_exception(answer.failure());
		FAIL() << "Expected stored task failure";
	}
	catch (const spk::Exception &exception)
	{
		EXPECT_STREQ(exception.what(), "expected task failure");
	}
}

TEST(TaskWorkerPool, RejectsZeroWorkers)
{
	EXPECT_THROW((void)spk::WorkerPool(0u), spk::Exception);
}
