#include "erelia/core/chunk.hpp"
#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/voxel/volume_builder.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <type_traits>
#include <utility>

static_assert(std::is_base_of_v<Voxel::Volume, Chunk>);
static_assert(std::is_base_of_v<Voxel::Volume::Builder, Chunk::Builder>);
static_assert(std::is_same_v<decltype(std::declval<Chunk::Builder &&>().build()), Chunk>);

namespace
{
	template <typename TType>
	concept HasCoordinateAccessor = requires(const TType &value) {
		value.coordinate();
	};

	static_assert(!HasCoordinateAccessor<Chunk>);

	Voxel::Volume makeVolume(
		const spk::Vector3UInt &dimensions,
		Voxel::Volume::UnitSize unitSize)
	{
		Voxel::Volume::Builder builder(dimensions, unitSize);
		(void)builder.set({0, 0, 0}, Voxel::Cell(17u));
		return std::move(builder).build();
	}
}

TEST(ChunkBuilder, FixesChunkDimensionsAndUnitSize)
{
	Chunk::Builder builder;
	EXPECT_TRUE(builder.set({15, 15, 15}, Voxel::Cell(42u)));

	const Chunk chunk = std::move(builder).build();

	EXPECT_EQ(chunk.dimensions(), (spk::Vector3UInt{16, 16, 16}));
	EXPECT_EQ(chunk.unitSize(), 1.0f);
	ASSERT_EQ(chunk.cells().size(), 4096u);
	EXPECT_EQ(chunk.at({15, 15, 15}).packed(), 42u);
	EXPECT_THROW((void)chunk.at({16, 0, 0}), spk::Exception);
}

TEST(ChunkBuilder, DefaultBuiltChunkContainsOnlyEmptyCells)
{
	const Chunk chunk = std::move(Chunk::Builder{}).build();

	ASSERT_EQ(chunk.cells().size(), 4096u);
	for (const Voxel::Cell &cell : chunk.cells())
	{
		EXPECT_EQ(cell.packed(), Voxel::Cell::Empty.packed());
	}
}

TEST(Chunk, CheckedVolumePromotionAcceptsExactChunkInvariants)
{
	auto volume = makeVolume({16, 16, 16}, 1.0f);
	const Voxel::Cell *storage = volume.cells().data();

	const Chunk chunk(std::move(volume));

	EXPECT_EQ(chunk.dimensions(), (spk::Vector3UInt{16, 16, 16}));
	EXPECT_EQ(chunk.unitSize(), 1.0f);
	EXPECT_EQ(chunk.cells().data(), storage);
	EXPECT_EQ(chunk.at({0, 0, 0}).packed(), 17u);
	EXPECT_EQ(volume.dimensions(), (spk::Vector3UInt{0, 0, 0}));
	EXPECT_EQ(volume.unitSize(), 0.0f);
	EXPECT_TRUE(volume.cells().empty());
}

TEST(Chunk, CheckedVolumePromotionRejectsInvalidDimensionsWithoutConsumingSource)
{
	auto volume = makeVolume({16, 16, 15}, 1.0f);
	const Voxel::Cell *storage = volume.cells().data();

	EXPECT_THROW((void)Chunk(std::move(volume)), spk::Exception);

	EXPECT_EQ(volume.dimensions(), (spk::Vector3UInt{16, 16, 15}));
	EXPECT_EQ(volume.unitSize(), 1.0f);
	EXPECT_EQ(volume.cells().data(), storage);
}

TEST(Chunk, CheckedVolumePromotionRejectsInvalidUnitSizeWithoutConsumingSource)
{
	auto volume = makeVolume({16, 16, 16}, 0.5f);
	const Voxel::Cell *storage = volume.cells().data();

	EXPECT_THROW((void)Chunk(std::move(volume)), spk::Exception);

	EXPECT_EQ(volume.dimensions(), (spk::Vector3UInt{16, 16, 16}));
	EXPECT_EQ(volume.unitSize(), 0.5f);
	EXPECT_EQ(volume.cells().data(), storage);
}
