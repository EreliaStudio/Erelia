#include "voxel/chunk.hpp"

#include <gtest/gtest.h>
#include <exception.hpp>
#include <type_traits>

TEST(ChunkStorageTest, HasFixedDimensionsAndCoordinateIndexing)
{
	static_assert(voxel::Chunk::Size == 16);
	static_assert(voxel::Chunk::CellCount == 4096);
	EXPECT_EQ(voxel::Chunk::index({0, 0, 0}), 0);
	EXPECT_EQ(voxel::Chunk::index({15, 0, 0}), 15);
	EXPECT_EQ(voxel::Chunk::index({0, 0, 1}), 16);
	EXPECT_EQ(voxel::Chunk::index({0, 1, 0}), 256);
	EXPECT_EQ(voxel::Chunk::index({15, 15, 15}), 4095);
}

TEST(ChunkStorageTest, ExposesFixedVolumeMetadata)
{
	static_assert(std::is_base_of_v<voxel::VoxelVolume, voxel::Chunk>);
	const voxel::Chunk chunk({2, -1, 3});
	const auto bounds = chunk.localBounds();

	EXPECT_EQ(chunk.dimensions(), spk::Vector3UInt(16, 16, 16));
	EXPECT_FLOAT_EQ(chunk.voxelSize(), 1.0f);
	EXPECT_EQ(bounds.minimum, spk::Vector3());
	EXPECT_EQ(bounds.maximum, spk::Vector3(16.0f, 16.0f, 16.0f));
}

TEST(ChunkStorageTest, RejectsCoordinatesOutsideChunk)
{
	for (const auto outside : {spk::Vector3Int{-1, 0, 0}, {16, 0, 0}, {0, -1, 0}, {0, 16, 0}, {0, 0, -1}, {0, 0, 16}})
	{
		EXPECT_FALSE(voxel::Chunk::contains(outside));
		EXPECT_THROW(static_cast<void>(voxel::Chunk::index(outside)), spk::Exception);
	}
}
