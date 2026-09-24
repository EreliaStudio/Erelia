#include "voxel_test_utils.hpp"

#include "voxel/debug_chunk_generator.hpp"

#include <gtest/gtest.h>

#include <algorithm>

TEST(ChunkCollectionTest, RequestsGeneratesAndFindsChunk)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::Chunk::Collection chunks;
	voxel::Chunk::DebugGenerator generator(chunks, catalog, 1234);
	EXPECT_TRUE(chunks.request({-1, 0, 0}));
	EXPECT_FALSE(chunks.request({-1, 0, 0}));
	EXPECT_NE(chunks.find({-1, 0, 0}), nullptr);
}

TEST(ChunkCollectionTest, SeededGenerationIsIndependentFromRequestOrder)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::Chunk::Collection first;
	voxel::Chunk::DebugGenerator firstGenerator(first, catalog, 0xE7E11AULL);
	voxel::Chunk::Collection replay;
	voxel::Chunk::DebugGenerator replayGenerator(replay, catalog, 0xE7E11AULL);
	ASSERT_TRUE(first.request({-1, 0, 0}));
	ASSERT_TRUE(first.request({0, 0, 0}));
	ASSERT_TRUE(replay.request({0, 0, 0}));
	ASSERT_TRUE(replay.request({-1, 0, 0}));
	EXPECT_TRUE(std::ranges::equal(first.find({-1, 0, 0})->cells(), replay.find({-1, 0, 0})->cells()));
	EXPECT_TRUE(std::ranges::equal(first.find({0, 0, 0})->cells(), replay.find({0, 0, 0})->cells()));
}
