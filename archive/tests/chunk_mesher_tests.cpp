#include "voxel_test_utils.hpp"

#include "voxel/chunk_mesher.hpp"

#include <gtest/gtest.h>

#include <cmath>

TEST(ChunkMesherTest, PropagatesMaterialSlotsIntoAtlasUvs)
{
	auto content = playground_test::loadVoxelCatalog();
	voxel::Chunk::Collection chunks;
	auto chunk = playground_test::chunkWithCell({0, 0, 0}, {0, 0, 0}, voxel::Voxel::Cell(content.id("grass")));
	voxel::Chunk *source = chunk.get();
	ASSERT_TRUE(playground_test::publish(chunks, std::move(chunk)));
	const auto mesh = voxel::Chunk::Mesher(content, chunks).bake(*source);
	const auto vertices = mesh.layout().vertexBuffer().cast<spk::Texture3DVertex>();
	ASSERT_EQ(mesh.indexCount(), 36);
	ASSERT_EQ(vertices.size(), 24);
	bool sideUv = false, bottomUv = false, topUv = false;
	for (const auto &vertex : vertices)
	{
		if (std::abs(std::abs(vertex.normal.x) + std::abs(vertex.normal.z) - 1.0f) < 0.0001f &&
			vertex.uv.x >= 0.25f && vertex.uv.x <= 0.5f && vertex.uv.y >= 0.0f && vertex.uv.y <= 0.25f)
			sideUv = true;
		if (vertex.normal == spk::Vector3(0.0f, -1.0f, 0.0f) && vertex.uv.x >= 0.5f && vertex.uv.x <= 0.75f)
			bottomUv = true;
		if (vertex.normal == spk::Vector3(0.0f, 1.0f, 0.0f) && vertex.uv.x >= 0.0f && vertex.uv.x <= 0.25f)
			topUv = true;
	}
	EXPECT_TRUE(sideUv);
	EXPECT_TRUE(bottomUv);
	EXPECT_TRUE(topUv);
}

TEST(ChunkMesherTest, ResolvesAdjacentChunkAndPartialOcclusion)
{
	auto content = playground_test::loadVoxelCatalog();
	voxel::Chunk::Collection chunks;
	auto left = playground_test::chunkWithCell({0, 0, 0}, {15, 0, 0}, voxel::Voxel::Cell(content.id("stone")));
	auto right = playground_test::chunkWithCell({1, 0, 0}, {0, 0, 0}, voxel::Voxel::Cell(content.id("stone")));
	voxel::Chunk *leftSource = left.get();
	voxel::Chunk *rightSource = right.get();
	ASSERT_TRUE(playground_test::publish(chunks, std::move(left)));
	ASSERT_TRUE(playground_test::publish(chunks, std::move(right)));
	EXPECT_EQ(chunks.worldCell({15, 0, 0}), voxel::Voxel::Cell(content.id("stone")));
	EXPECT_EQ(chunks.worldCell({16, 0, 0}), voxel::Voxel::Cell(content.id("stone")));
	EXPECT_FALSE(chunks.worldCell({32, 0, 0}).has_value());
	voxel::Chunk::Mesher mesher(content, chunks);
	EXPECT_EQ(mesher.bake(*leftSource).indexCount(), 30);
	EXPECT_EQ(mesher.bake(*rightSource).indexCount(), 30);

	voxel::Chunk::Collection partial;
	auto slab = playground_test::chunkWithCell({0, 0, 0}, {0, 0, 0}, voxel::Voxel::Cell(content.id("debug_slab")));
	{
		auto editor = slab->edit();
		editor.set({1, 0, 0}, voxel::Voxel::Cell(content.id("stone")));
	}
	voxel::Chunk *partialSource = slab.get();
	ASSERT_TRUE(playground_test::publish(partial, std::move(slab)));
	EXPECT_EQ(voxel::Chunk::Mesher(content, partial).bake(*partialSource).indexCount(), 66);
}
