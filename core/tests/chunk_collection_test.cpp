#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/chunk_collection.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <atomic>
#include <cstddef>
#include <exception>
#include <memory>
#include <mutex>
#include <optional>
#include <stdexcept>
#include <thread>
#include <type_traits>
#include <utility>
#include <vector>

namespace
{
	struct ProviderState final
	{
		std::mutex mutex;
		std::vector<Chunk::Coordinate> requests;
		std::vector<
			std::pair<
				Chunk::Coordinate,
				std::shared_ptr<spk::Task<Chunk>>>>
			tasks;
		std::optional<Chunk::Coordinate> throwingCoordinate;
	};

	class TestProvider final : public Chunk::Collection::Provider
	{
	private:
		std::shared_ptr<ProviderState> _state;

	public:
		explicit TestProvider(
			std::shared_ptr<ProviderState> state) :
			_state(std::move(state))
		{
		}

		TestProvider(const TestProvider &) = delete;
		TestProvider &operator=(
			const TestProvider &) = delete;
		TestProvider(TestProvider &&) noexcept = default;
		TestProvider &operator=(
			TestProvider &&) noexcept = default;

		[[nodiscard]] spk::Task<Chunk>::Answer request(
			const Chunk::Coordinate &coordinate) override
		{
			const std::scoped_lock lock(_state->mutex);
			_state->requests.push_back(coordinate);

			if (
				_state->throwingCoordinate.has_value() &&
				*_state->throwingCoordinate == coordinate)
			{
				throw std::runtime_error(
					"provider scheduling failure");
			}

			auto task =
				std::make_shared<spk::Task<Chunk>>();
			const auto answer = task->answer();
			_state->tasks.emplace_back(
				coordinate,
				std::move(task));
			return answer;
		}
	};

	static_assert(
		std::is_constructible_v<
			Chunk::Collection,
			TestProvider &&>);
	static_assert(
		!std::is_constructible_v<
			Chunk::Collection,
			TestProvider &>);

	Chunk makeChunk(Voxel::Cell::PackedType packed)
	{
		Chunk::Builder builder;
		(void)builder.set(
			{0, 0, 0},
			Voxel::Cell(packed));
		return std::move(builder).build();
	}

	std::vector<Chunk::Coordinate> requests(
		const std::shared_ptr<ProviderState> &state)
	{
		const std::scoped_lock lock(state->mutex);
		return state->requests;
	}

	std::shared_ptr<spk::Task<Chunk>> taskFor(
		const std::shared_ptr<ProviderState> &state,
		const Chunk::Coordinate &coordinate)
	{
		const std::scoped_lock lock(state->mutex);
		for (const auto &[current, task] : state->tasks)
		{
			if (current == coordinate)
			{
				return task;
			}
		}
		return nullptr;
	}

	const Chunk::Collection::BatchResult::Acquired *
	findAcquired(
		const Chunk::Collection::BatchResult &result,
		const Chunk::Coordinate &coordinate)
	{
		for (const auto &current : result.acquired)
		{
			if (current.coordinate == coordinate)
			{
				return &current;
			}
		}
		return nullptr;
	}

	const Chunk::Collection::BatchResult::Failed *
	findFailed(
		const Chunk::Collection::BatchResult &result,
		const Chunk::Coordinate &coordinate)
	{
		for (const auto &current : result.failed)
		{
			if (current.coordinate == coordinate)
			{
				return &current;
			}
		}
		return nullptr;
	}
}

TEST(ChunkCollection, EmptyBatchCompletesImmediately)
{
	auto providerState =
		std::make_shared<ProviderState>();
	Chunk::Collection collection{
		TestProvider(providerState)};

	const auto answer =
		collection.request({});

	ASSERT_EQ(
		answer.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Completed);
	EXPECT_TRUE(answer.result().acquired.empty());
	EXPECT_TRUE(answer.result().failed.empty());
	EXPECT_TRUE(requests(providerState).empty());
}

TEST(ChunkCollection, MissingCoordinateBecomesPendingThenAvailable)
{
	auto providerState =
		std::make_shared<ProviderState>();
	Chunk::Collection collection{
		TestProvider(providerState)};
	const Chunk::Coordinate coordinate{3, 0, -2};

	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Absent);

	const auto answer =
		collection.request({coordinate});

	EXPECT_EQ(
		answer.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Pending);
	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Pending);
	EXPECT_FALSE(
		collection.tryGet(coordinate).has_value());

	const auto recorded = requests(providerState);
	ASSERT_EQ(recorded.size(), 1u);
	EXPECT_EQ(recorded.front(), coordinate);

	auto task = taskFor(
		providerState,
		coordinate);
	ASSERT_NE(task, nullptr);
	task->validate(makeChunk(101u));

	ASSERT_EQ(
		answer.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Completed);
	ASSERT_EQ(answer.result().acquired.size(), 1u);
	EXPECT_TRUE(answer.result().failed.empty());
	EXPECT_EQ(
		answer.result().acquired.front().coordinate,
		coordinate);
	EXPECT_EQ(
		answer.result().acquired.front().chunk.at({0, 0, 0}).packed(),
		101u);

	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Available);
	const auto stored =
		collection.tryGet(coordinate);
	ASSERT_TRUE(stored.has_value());
	EXPECT_EQ(
		stored->at({0, 0, 0}).packed(),
		101u);
}

TEST(ChunkCollection, AvailableCoordinateCompletesWithoutProviderCall)
{
	auto providerState =
		std::make_shared<ProviderState>();
	Chunk::Collection collection{
		TestProvider(providerState)};
	const Chunk::Coordinate coordinate{4, 1, 7};

	collection.replace(
		coordinate,
		makeChunk(222u));

	const auto answer =
		collection.request({coordinate});

	ASSERT_EQ(
		answer.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Completed);
	ASSERT_EQ(answer.result().acquired.size(), 1u);
	EXPECT_EQ(
		answer.result().acquired.front().chunk.at({0, 0, 0}).packed(),
		222u);
	EXPECT_TRUE(requests(providerState).empty());
}

TEST(ChunkCollection, OverlappingBatchesReusePendingCoordinateTask)
{
	auto providerState =
		std::make_shared<ProviderState>();
	Chunk::Collection collection{
		TestProvider(providerState)};
	const Chunk::Coordinate coordinate{9, 2, -6};

	const auto first =
		collection.request({coordinate});
	const auto second =
		collection.request({coordinate});

	ASSERT_EQ(requests(providerState).size(), 1u);
	EXPECT_EQ(
		first.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Pending);
	EXPECT_EQ(
		second.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Pending);

	taskFor(providerState, coordinate)
		->validate(makeChunk(333u));

	ASSERT_EQ(
		first.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Completed);
	ASSERT_EQ(
		second.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Completed);
	EXPECT_EQ(first.result().acquired.size(), 1u);
	EXPECT_EQ(second.result().acquired.size(), 1u);
	EXPECT_EQ(requests(providerState).size(), 1u);
}

TEST(ChunkCollection, BatchWaitsForEveryCoordinateAndPreservesMixedOutcomes)
{
	auto providerState =
		std::make_shared<ProviderState>();
	Chunk::Collection collection{
		TestProvider(providerState)};
	const Chunk::Coordinate successCoordinate{1, 2, 3};
	const Chunk::Coordinate failureCoordinate{-1, -2, -3};

	const auto answer =
		collection.request(
			{successCoordinate, failureCoordinate});

	auto failureTask =
		taskFor(
			providerState,
			failureCoordinate);
	ASSERT_NE(failureTask, nullptr);
	failureTask->fail(
		std::make_exception_ptr(
			std::runtime_error(
				"generation failure")));

	EXPECT_EQ(
		answer.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Pending);
	EXPECT_EQ(
		collection.state(failureCoordinate),
		Chunk::Collection::State::Absent);

	auto successTask =
		taskFor(
			providerState,
			successCoordinate);
	ASSERT_NE(successTask, nullptr);
	successTask->validate(makeChunk(444u));

	ASSERT_EQ(
		answer.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Completed);
	const auto &result = answer.result();
	ASSERT_EQ(result.acquired.size(), 1u);
	ASSERT_EQ(result.failed.size(), 1u);

	const auto *acquired =
		findAcquired(result, successCoordinate);
	ASSERT_NE(acquired, nullptr);
	EXPECT_EQ(
		acquired->chunk.at({0, 0, 0}).packed(),
		444u);

	const auto *failed =
		findFailed(result, failureCoordinate);
	ASSERT_NE(failed, nullptr);
	EXPECT_THROW(
		std::rethrow_exception(failed->exception),
		std::runtime_error);

	EXPECT_EQ(
		collection.state(successCoordinate),
		Chunk::Collection::State::Available);
	EXPECT_EQ(
		collection.state(failureCoordinate),
		Chunk::Collection::State::Absent);
}

TEST(ChunkCollection, ProviderSchedulingExceptionIsPerCoordinateFailure)
{
	auto providerState =
		std::make_shared<ProviderState>();
	const Chunk::Coordinate coordinate{7, 8, 9};
	providerState->throwingCoordinate =
		coordinate;
	Chunk::Collection collection{
		TestProvider(providerState)};

	const auto answer =
		collection.request({coordinate});

	ASSERT_EQ(
		answer.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Completed);
	EXPECT_TRUE(answer.result().acquired.empty());
	ASSERT_EQ(answer.result().failed.size(), 1u);
	EXPECT_EQ(
		answer.result().failed.front().coordinate,
		coordinate);
	EXPECT_THROW(
		std::rethrow_exception(
			answer.result().failed.front().exception),
		std::runtime_error);
	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Absent);
}

TEST(ChunkCollection, FailedCoordinateCanBeRequestedAgain)
{
	auto providerState =
		std::make_shared<ProviderState>();
	Chunk::Collection collection{
		TestProvider(providerState)};
	const Chunk::Coordinate coordinate{2, 3, 4};

	const auto first =
		collection.request({coordinate});
	taskFor(providerState, coordinate)
		->fail(
			std::make_exception_ptr(
				std::runtime_error("failure")));
	ASSERT_EQ(
		first.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Completed);
	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Absent);

	const auto second =
		collection.request({coordinate});
	ASSERT_EQ(requests(providerState).size(), 2u);
	EXPECT_EQ(
		second.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Pending);
}

TEST(ChunkCollection, StalePendingCompletionCannotOverwriteReplacement)
{
	auto providerState =
		std::make_shared<ProviderState>();
	Chunk::Collection collection{
		TestProvider(providerState)};
	const Chunk::Coordinate coordinate{5, 6, 7};

	const auto answer =
		collection.request({coordinate});
	auto oldTask =
		taskFor(providerState, coordinate);
	ASSERT_NE(oldTask, nullptr);

	collection.replace(
		coordinate,
		makeChunk(900u));
	oldTask->validate(makeChunk(100u));

	ASSERT_EQ(
		answer.status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Completed);
	const auto stored =
		collection.tryGet(coordinate);
	ASSERT_TRUE(stored.has_value());
	EXPECT_EQ(
		stored->at({0, 0, 0}).packed(),
		900u);
}

TEST(ChunkCollection, ReplacementOfAbsentCoordinateUpsertsWithoutProviderCall)
{
	auto providerState =
		std::make_shared<ProviderState>();
	Chunk::Collection collection{
		TestProvider(providerState)};
	const Chunk::Coordinate coordinate{-9, 4, 11};

	collection.replace(
		coordinate,
		makeChunk(777u));
	const auto stored =
		collection.tryGet(coordinate);

	EXPECT_TRUE(requests(providerState).empty());
	ASSERT_TRUE(stored.has_value());
	EXPECT_EQ(
		stored->at({0, 0, 0}).packed(),
		777u);
	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Available);
}

TEST(ChunkCollection, ReplacementPublishesNewValueWithoutMutatingOldCopy)
{
	auto providerState =
		std::make_shared<ProviderState>();
	Chunk::Collection collection{
		TestProvider(providerState)};
	const Chunk::Coordinate coordinate{1, 2, 3};

	collection.replace(
		coordinate,
		makeChunk(200u));
	const auto oldOptional =
		collection.tryGet(coordinate);
	ASSERT_TRUE(oldOptional.has_value());
	const Chunk oldCopy = *oldOptional;
	const Voxel::Cell *oldStorage =
		oldCopy.cells().data();

	collection.replace(
		coordinate,
		makeChunk(300u));
	const auto replacement =
		collection.tryGet(coordinate);

	ASSERT_TRUE(replacement.has_value());
	EXPECT_EQ(oldCopy.cells().data(), oldStorage);
	EXPECT_EQ(
		oldCopy.at({0, 0, 0}).packed(),
		200u);
	EXPECT_EQ(
		replacement->at({0, 0, 0}).packed(),
		300u);
	EXPECT_NE(
		replacement->cells().data(),
		oldStorage);
}

TEST(ChunkCollection, ConcurrentLookupAndReplacementReturnPublishedSnapshots)
{
	auto providerState =
		std::make_shared<ProviderState>();
	Chunk::Collection collection{
		TestProvider(providerState)};
	const Chunk::Coordinate coordinate{5, -2, 8};
	const Chunk first = makeChunk(200u);
	const Chunk second = makeChunk(300u);

	collection.replace(coordinate, first);

	std::atomic<bool> failed = false;
	std::vector<std::thread> readers;
	for (std::size_t threadIndex = 0u;
		 threadIndex < 6u;
		 ++threadIndex)
	{
		readers.emplace_back(
			[&] {
				for (
					std::size_t iteration = 0u;
					iteration < 1000u;
					++iteration)
				{
					const auto snapshot =
						collection.tryGet(coordinate);
					if (!snapshot.has_value())
					{
						failed.store(
							true,
							std::memory_order_relaxed);
						return;
					}

					const auto packed =
						snapshot->at({0, 0, 0})
							.packed();
					if (
						packed != 200u &&
						packed != 300u)
					{
						failed.store(
							true,
							std::memory_order_relaxed);
						return;
					}
				}
			});
	}

	std::thread writer(
		[&] {
			for (
				std::size_t iteration = 0u;
				iteration < 1000u;
				++iteration)
			{
				collection.replace(
					coordinate,
					iteration % 2u == 0u
						? first
						: second);
			}
		});

	for (std::thread &reader : readers)
	{
		reader.join();
	}
	writer.join();

	EXPECT_FALSE(
		failed.load(std::memory_order_relaxed));
	EXPECT_TRUE(requests(providerState).empty());
}


TEST(ChunkCollection, CompletionAfterCollectionDestructionDoesNotAccessDestroyedState)
{
	auto providerState =
		std::make_shared<ProviderState>();
	const Chunk::Coordinate coordinate{12, -4, 9};

	std::optional<
		spk::Task<Chunk::Collection::BatchResult>::Answer>
		answer;
	{
		Chunk::Collection collection{
			TestProvider(providerState)};
		answer.emplace(
			collection.request({coordinate}));
		EXPECT_EQ(
			answer->status(),
			spk::Task<Chunk::Collection::BatchResult>::
				Status::Pending);
	}

	auto task = taskFor(
		providerState,
		coordinate);
	ASSERT_NE(task, nullptr);
	task->validate(makeChunk(515u));

	ASSERT_EQ(
		answer->status(),
		spk::Task<Chunk::Collection::BatchResult>::
			Status::Completed);
	ASSERT_EQ(answer->result().acquired.size(), 1u);
	EXPECT_EQ(
		answer->result().acquired.front().coordinate,
		coordinate);
	EXPECT_EQ(
		answer->result().acquired.front().chunk.at({0, 0, 0}).packed(),
		515u);
}
