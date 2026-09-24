#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/chunk_collection.hpp"

#include <gtest/gtest.h>

#include <atomic>
#include <cstddef>
#include <cstdint>
#include <memory>
#include <thread>
#include <type_traits>
#include <utility>
#include <vector>

namespace
{
	struct ProviderState
	{
		std::vector<Chunk::Coordinate> requests;
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

		[[nodiscard]] Chunk provide(
			const Chunk::Coordinate &coordinate) override
		{
			_state->requests.push_back(coordinate);

			Chunk::Builder builder;
			const auto packed = static_cast<Voxel::Cell::PackedType>(
				100u + _state->requests.size());
			(void)builder.set({0, 0, 0}, Voxel::Cell(packed));
			return std::move(builder).build();
		}
	};

	static_assert(std::is_constructible_v<Chunk::Collection, TestProvider &&>);
	static_assert(!std::is_constructible_v<Chunk::Collection, TestProvider &>);

	Chunk makeChunk(Voxel::Cell::PackedType packed)
	{
		Chunk::Builder builder;
		(void)builder.set({0, 0, 0}, Voxel::Cell(packed));
		return std::move(builder).build();
	}
}

TEST(ChunkCollection, MissingCoordinateInvokesOwnedProviderOnceAndCachesResult)
{
	auto state = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(state)};
	const Chunk::Coordinate coordinate{3, 0, -2};

	const Chunk first = collection.get(coordinate);
	const Chunk second = collection.get(coordinate);

	ASSERT_EQ(state->requests.size(), 1u);
	EXPECT_EQ(state->requests.front(), coordinate);
	EXPECT_EQ(first.at({0, 0, 0}).packed(), 101u);
	EXPECT_EQ(second.at({0, 0, 0}).packed(), 101u);
	EXPECT_EQ(first.cells().data(), second.cells().data());
}

TEST(ChunkCollection, DifferentPositiveAndNegativeCoordinatesAreCachedIndependently)
{
	auto state = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(state)};

	const Chunk positive = collection.get({4, 1, 7});
	const Chunk negative = collection.get({-4, -1, -7});

	ASSERT_EQ(state->requests.size(), 2u);
	EXPECT_EQ(state->requests[0], (Chunk::Coordinate{4, 1, 7}));
	EXPECT_EQ(state->requests[1], (Chunk::Coordinate{-4, -1, -7}));
	EXPECT_EQ(positive.at({0, 0, 0}).packed(), 101u);
	EXPECT_EQ(negative.at({0, 0, 0}).packed(), 102u);

	(void)collection.get({4, 1, 7});
	(void)collection.get({-4, -1, -7});
	EXPECT_EQ(state->requests.size(), 2u);
}

TEST(ChunkCollection, ReplacementPublishesCompleteNewValueWithoutMutatingOldCopy)
{
	auto state = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(state)};
	const Chunk::Coordinate coordinate{1, 2, 3};

	const Chunk oldCopy = collection.get(coordinate);
	const Voxel::Cell *oldStorage = oldCopy.cells().data();

	collection.replace(coordinate, makeChunk(900u));
	const Chunk replacement = collection.get(coordinate);

	EXPECT_EQ(state->requests.size(), 1u);
	EXPECT_EQ(oldCopy.at({0, 0, 0}).packed(), 101u);
	EXPECT_EQ(oldCopy.cells().data(), oldStorage);
	EXPECT_EQ(replacement.at({0, 0, 0}).packed(), 900u);
	EXPECT_NE(replacement.cells().data(), oldStorage);
}

TEST(ChunkCollection, ReplacementOfAbsentCoordinateUpsertsWithoutProviderCall)
{
	auto state = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(state)};
	const Chunk::Coordinate coordinate{-9, 4, 11};

	collection.replace(coordinate, makeChunk(777u));
	const Chunk stored = collection.get(coordinate);

	EXPECT_TRUE(state->requests.empty());
	EXPECT_EQ(stored.at({0, 0, 0}).packed(), 777u);
}

TEST(ChunkCollection, ConcurrentLookupAndReplacementReturnOnlyPublishedSnapshots)
{
	auto state = std::make_shared<ProviderState>();
	Chunk::Collection collection{TestProvider(state)};
	const Chunk::Coordinate coordinate{5, -2, 8};
	const Chunk first = makeChunk(200u);
	const Chunk second = makeChunk(300u);

	collection.replace(coordinate, first);
	const Chunk retained = collection.get(coordinate);
	const Voxel::Cell *retainedStorage = retained.cells().data();

	std::atomic<bool> failed = false;
	std::vector<std::thread> readers;
	for (std::size_t threadIndex = 0; threadIndex < 6u; ++threadIndex)
	{
		readers.emplace_back([&] {
			for (std::size_t iteration = 0; iteration < 1000u; ++iteration)
			{
				const Chunk snapshot = collection.get(coordinate);
				const auto packed = snapshot.at({0, 0, 0}).packed();
				if (packed != 200u && packed != 300u)
				{
					failed.store(true, std::memory_order_relaxed);
					return;
				}
			}
		});
	}

	std::thread writer([&] {
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
	EXPECT_TRUE(state->requests.empty());
	EXPECT_EQ(retained.at({0, 0, 0}).packed(), 200u);
	EXPECT_EQ(retained.cells().data(), retainedStorage);
}
