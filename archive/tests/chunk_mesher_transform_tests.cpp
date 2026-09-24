#include "voxel_test_utils.hpp"

#include "voxel/chunk_mesher.hpp"

#include <gtest/gtest.h>

namespace
{
	void expectCounterClockwise(const spk::TextureMesh3D &mesh)
	{
		const auto vertices = mesh.layout().vertexBuffer().cast<spk::Texture3DVertex>();
		const auto indices = mesh.layout().indexBuffer().cast<spk::TextureMesh3D::Index>();
		ASSERT_EQ(indices.size() % 3, 0);
		for (std::size_t index = 0; index < indices.size(); index += 3)
		{
			const auto &a = vertices[indices[index]];
			const auto &b = vertices[indices[index + 1]];
			const auto &c = vertices[indices[index + 2]];
			EXPECT_GT((b.position - a.position).cross(c.position - a.position).dot(a.normal), 0.0f);
		}
	}
}

TEST(ChunkMesherTransformTest, VerticalFlipChangesBoundaryOcclusion)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::Chunk::Collection chunks;
	auto lower = playground_test::chunkWithCell({0, 0, 0}, {15, 0, 0}, voxel::Voxel::Cell(catalog.id("debug_slab")));
	auto upper = playground_test::chunkWithCell({1, 0, 0}, {0, 0, 0},
		voxel::Voxel::Cell(catalog.id("debug_slab"), voxel::Voxel::Orientation::PositiveZ, voxel::Voxel::Flip::NegativeY));
	voxel::Chunk *lowerSource = lower.get();
	voxel::Chunk *upperSource = upper.get();
	ASSERT_TRUE(playground_test::publish(chunks, std::move(lower)));
	ASSERT_TRUE(playground_test::publish(chunks, std::move(upper)));
	voxel::Chunk::Mesher mesher(catalog, chunks);
	EXPECT_EQ(mesher.bake(*lowerSource).indexCount(), 36);
	EXPECT_EQ(mesher.bake(*upperSource).indexCount(), 36);
}

TEST(ChunkMesherTransformTest, PreservesCounterClockwiseWindingForAllTransforms)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::Chunk::Collection chunks;
	auto transformed = std::make_unique<voxel::Chunk>(voxel::Chunk::Coordinate{0, 0, 0});
	{
		auto editor = transformed->edit();
		for (std::uint32_t transform = 0; transform < 8; ++transform)
		{
			const auto orientation = static_cast<voxel::Voxel::Orientation>((transform % 4) << 29);
			const auto flip = transform < 4 ? voxel::Voxel::Flip::PositiveY : voxel::Voxel::Flip::NegativeY;
			const int x = static_cast<int>((transform % 4) * 3);
			const int y = static_cast<int>((transform / 4) * 3);
			editor.set({x, y, 0}, voxel::Voxel::Cell(catalog.id("debug_slope"), orientation, flip));
			editor.set({x, y, 3}, voxel::Voxel::Cell(catalog.id("debug_stair"), orientation, flip));
			editor.set({x, y, 6}, voxel::Voxel::Cell(catalog.id("debug_slab"), orientation, flip));
		}
	}
	voxel::Chunk *source = transformed.get();
	ASSERT_TRUE(playground_test::publish(chunks, std::move(transformed)));
	expectCounterClockwise(voxel::Chunk::Mesher(catalog, chunks).bake(*source));
}
