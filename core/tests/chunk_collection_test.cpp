#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/chunk_collection.hpp"

#include <gtest/gtest.h>

#include <atomic>
#include <cstddef>
#include <cstdint>
#include <memory>
#include <mutex>
#include <thread>
#include <type_traits>
#include <utility>
#include <vector>

namespace
{
	struct ProviderState
	{
		std::mutex mutex;
		std::vector<Chunk::Collection::Request> requests;
		std::vector<Chunk::Collection::Request> pending;
	};

	class TestProvider final : public Chunk::Collection::Provider
	{
	private:
		std::shared_ptr<ProviderState> _state;

	public:
		explicit TestProvider(std::shared_ptr<ProviderState> state) :
			_state(std::move(state))
		{
		}

		TestProvider(const TestProvider &) = delete;
		TestProvider &operator=(const TestProvider &) = delete;
		TestProvider(TestProvider &&) noexcept = default;
		TestProvider &operator=(TestProvider &&) noexcept = default;

		void request(
			const Chunk::Collection::Request &request) override
		{
			const std::scoped_lock lock(_state->mutex);
			_state->requests.push_back(request);
			_state->pending.push_back(request);
		}

		void update(Chunk::Collection &collection) override
		{
			std::vector<Chunk::Collection::Request> pending;
			{
				const std::scoped_lock lock(_state->mutex);
				pending.swap(_state->pending);
			}

			for (const Chunk::Collection::Request &request : pending)
			{
				Chunk::Builder builder;
				const auto packed =
					static_cast<Voxel::Cell::PackedType>(
						100u + request.generation);
				(void)builder.set(
					{0, 0, 0},
					Voxel::Cell(packed));
				(void)collection.publish(
					request,
					std::move(builder).build());
			}
		}
	};

	static_assert(
		std::is_constructible_v<Chunk::Collection, TestProvider &&>);
	static_assert(
		!std::is_constructible_v<Chunk::Collection, TestProvider &>);

	Chunk makeChunk(Voxel::Cell::PackedType packed)
	{
		Chunk::Builder builder;
		(void)builder.set({0, 0, 0}, Voxel::Cell(packed));
		return std::move(builder).build();
	}

	std::vector<Chunk::Collection::Request> requests(
		const std::shared_ptr<ProviderState> &state)
	{
		const std::scoped_lock lock(state->mutex);
		return state->requests;
	}
}

TEST(ChunkCollection, MissingCoordinateBecomesPendingThenAvailable)
{
	auto providerState = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(providerState)};
	const Chunk::Coordinate coordinate{3, 0, -2};

	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Absent);
	EXPECT_TRUE(collection.request(coordinate));
	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Pending);
	EXPECT_FALSE(collection.tryGet(coordinate).has_value());
	EXPECT_FALSE(collection.request(coordinate));

	const auto recorded = requests(providerState);
	ASSERT_EQ(recorded.size(), 1u);
	EXPECT_EQ(recorded.front().coordinate, coordinate);

	collection.update();

	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Available);
	const auto chunk = collection.tryGet(coordinate);
	ASSERT_TRUE(chunk.has_value());
	EXPECT_EQ(
		chunk->at({0, 0, 0}).packed(),
		static_cast<Voxel::Cell::PackedType>(
			100u + recorded.front().generation));
	EXPECT_FALSE(collection.request(coordinate));
	EXPECT_EQ(requests(providerState).size(), 1u);
}

TEST(ChunkCollection, DifferentCoordinatesAreRequestedIndependently)
{
	auto providerState = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(providerState)};

	EXPECT_TRUE(collection.request({4, 1, 7}));
	EXPECT_TRUE(collection.request({-4, -1, -7}));
	EXPECT_EQ(requests(providerState).size(), 2u);

	collection.update();

	EXPECT_TRUE(collection.tryGet({4, 1, 7}).has_value());
	EXPECT_TRUE(collection.tryGet({-4, -1, -7}).has_value());
	EXPECT_EQ(requests(providerState).size(), 2u);
}

TEST(ChunkCollection, ConcurrentDuplicateRequestOnlyReachesProviderOnce)
{
	auto providerState = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(providerState)};
	const Chunk::Coordinate coordinate{9, 2, -6};

	std::atomic<std::size_t> accepted = 0u;
	std::vector<std::thread> threads;
	for (std::size_t index = 0; index < 8u; ++index)
	{
		threads.emplace_back(
			[&] {
				if (collection.request(coordinate))
				{
					accepted.fetch_add(
						1u,
						std::memory_order_relaxed);
				}
			});
	}

	for (std::thread &thread : threads)
	{
		thread.join();
	}

	EXPECT_EQ(accepted.load(std::memory_order_relaxed), 1u);
	EXPECT_EQ(requests(providerState).size(), 1u);
	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Pending);
}

TEST(ChunkCollection, StaleGenerationCannotPublishOverNewRequest)
{
	auto providerState = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(providerState)};
	const Chunk::Coordinate coordinate{2, 3, 4};

	ASSERT_TRUE(collection.request(coordinate));
	auto recorded = requests(providerState);
	ASSERT_EQ(recorded.size(), 1u);
	const Chunk::Collection::Request first = recorded.front();

	EXPECT_TRUE(collection.fail(first));
	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Absent);

	ASSERT_TRUE(collection.request(coordinate));
	recorded = requests(providerState);
	ASSERT_EQ(recorded.size(), 2u);
	const Chunk::Collection::Request second = recorded.back();
	EXPECT_NE(first.generation, second.generation);

	collection.update();

	const auto chunk = collection.tryGet(coordinate);
	ASSERT_TRUE(chunk.has_value());
	EXPECT_EQ(
		chunk->at({0, 0, 0}).packed(),
		static_cast<Voxel::Cell::PackedType>(
			100u + second.generation));
	EXPECT_FALSE(collection.publish(first, makeChunk(999u)));
}

TEST(ChunkCollection, ReplacementPublishesNewValueWithoutMutatingOldCopy)
{
	auto providerState = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(providerState)};
	const Chunk::Coordinate coordinate{1, 2, 3};

	ASSERT_TRUE(collection.request(coordinate));
	collection.update();
	const auto oldOptional = collection.tryGet(coordinate);
	ASSERT_TRUE(oldOptional.has_value());
	const Chunk oldCopy = *oldOptional;
	const Voxel::Cell *oldStorage = oldCopy.cells().data();

	collection.replace(coordinate, makeChunk(900u));
	const auto replacement = collection.tryGet(coordinate);

	ASSERT_TRUE(replacement.has_value());
	EXPECT_EQ(oldCopy.cells().data(), oldStorage);
	EXPECT_EQ(replacement->at({0, 0, 0}).packed(), 900u);
	EXPECT_NE(replacement->cells().data(), oldStorage);
}

TEST(ChunkCollection, ReplacementOfAbsentCoordinateUpsertsWithoutProviderCall)
{
	auto providerState = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(providerState)};
	const Chunk::Coordinate coordinate{-9, 4, 11};

	collection.replace(coordinate, makeChunk(777u));
	const auto stored = collection.tryGet(coordinate);

	EXPECT_TRUE(requests(providerState).empty());
	ASSERT_TRUE(stored.has_value());
	EXPECT_EQ(stored->at({0, 0, 0}).packed(), 777u);
	EXPECT_EQ(
		collection.state(coordinate),
		Chunk::Collection::State::Available);
}

TEST(ChunkCollection, ConcurrentLookupAndReplacementReturnPublishedSnapshots)
{
	auto providerState = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(providerState)};
	const Chunk::Coordinate coordinate{5, -2, 8};
	const Chunk first = makeChunk(200u);
	const Chunk second = makeChunk(300u);

	collection.replace(coordinate, first);
	const auto retainedOptional = collection.tryGet(coordinate);
	ASSERT_TRUE(retainedOptional.has_value());
	const Chunk retained = *retainedOptional;
	const Voxel::Cell *retainedStorage = retained.cells().data();

	std::atomic<bool> failed = false;
	std::vector<std::thread> readers;
	for (std::size_t threadIndex = 0; threadIndex < 6u; ++threadIndex)
	{
		readers.emplace_back(
			[&] {
				for (std::size_t iteration = 0; iteration < 1000u; ++iteration)
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
						snapshot->at({0, 0, 0}).packed();
					if (packed != 200u && packed != 300u)
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
			for (std::size_t iteration = 0; iteration < 1000u; ++iteration)
			{
				collection.replace(
					coordinate,
					iteration % 2u == 0u ? first : second);
			}
		});

	for (std::thread &reader : readers)
	{
		reader.join();
	}
	writer.join();

	EXPECT_FALSE(failed.load(std::memory_order_relaxed));
	EXPECT_TRUE(requests(providerState).empty());
	EXPECT_EQ(retained.at({0, 0, 0}).packed(), 200u);
	EXPECT_EQ(retained.cells().data(), retainedStorage);
}
