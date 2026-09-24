#include "voxel/chunk.hpp"

#include <gtest/gtest.h>

TEST(ChunkCoordinatesTest, ConvertsPositiveAndNegativeWorldCoordinates)
{
	struct Case
	{
		spk::Vector3Int world;
		voxel::Chunk::Coordinate chunk;
		spk::Vector3Int local;
	};
	const Case cases[] = {
		{{0, 0, 0}, {0, 0, 0}, {0, 0, 0}},
		{{15, 15, 15}, {0, 0, 0}, {15, 15, 15}},
		{{16, 16, 16}, {1, 1, 1}, {0, 0, 0}},
		{{-1, -1, -1}, {-1, -1, -1}, {15, 15, 15}},
		{{-16, -16, -16}, {-1, -1, -1}, {0, 0, 0}},
		{{-17, -17, -17}, {-2, -2, -2}, {15, 15, 15}}};
	for (const auto &value : cases)
	{
		EXPECT_EQ(voxel::Chunk::worldToChunk(value.world), value.chunk);
		EXPECT_EQ(voxel::Chunk::worldToLocal(value.world), value.local);
		EXPECT_EQ(voxel::Chunk::worldOrigin(value.chunk) + value.local, value.world);
	}
}
