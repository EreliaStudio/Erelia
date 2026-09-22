#include "semantic_mesh_snapshot.hpp"
#include "voxel_test_utils.hpp"

#include "voxel/chunk_mesher.hpp"
#include "voxel/voxel_mesher.hpp"

#include <gtest/gtest.h>

#include <string>
#include <string_view>

namespace
{
	spk::TextureMesh3D bakeVolumeCell(
		std::string_view definition,
		spk::Vector3Int position,
		voxel::Voxel::Orientation orientation = voxel::Voxel::Orientation::PositiveZ,
		voxel::Voxel::Flip flip = voxel::Voxel::Flip::PositiveY)
	{
		auto catalog = playground_test::loadVoxelCatalog();
		voxel::VoxelVolume volume({16, 16, 16}, 1.0f);
		volume.edit().set(position, voxel::Voxel::Cell(catalog.id(std::string(definition)), orientation, flip));
		return voxel::VoxelMesher(catalog).bake(volume);
	}

	spk::TextureMesh3D bakeChunkBoundary(bool publishNeighbor)
	{
		auto catalog = playground_test::loadVoxelCatalog();
		voxel::Chunk::Collection chunks;
		auto left = playground_test::chunkWithCell({0, 0, 0}, {15, 2, 3}, voxel::Voxel::Cell(catalog.id("stone")));
		voxel::Chunk *source = left.get();
		if (!playground_test::publish(chunks, std::move(left))) throw spk::Exception("fixture publication failed");
		if (publishNeighbor)
		{
			auto right = playground_test::chunkWithCell({1, 0, 0}, {0, 2, 3}, voxel::Voxel::Cell(catalog.id("stone")));
			if (!playground_test::publish(chunks, std::move(right))) throw spk::Exception("fixture publication failed");
		}
		return voxel::Chunk::Mesher(catalog, chunks).bake(*source);
	}

	void expectSnapshot(const spk::TextureMesh3D &mesh, std::string_view name)
	{
		EXPECT_EQ(playground_test::semanticMeshSnapshot(mesh),
			playground_test::loadCurrentBakerSemanticSnapshot(name));
	}
}

TEST(VoxelMesherSemanticGoldenTest, CubeMatchesCurrentBaker)
{
	expectSnapshot(bakeVolumeCell("grass", {1, 1, 1}), "cube");
}

TEST(VoxelMesherSemanticGoldenTest, SlabMatchesCurrentBaker)
{
	expectSnapshot(bakeVolumeCell("debug_slab", {2, 1, 1}, voxel::Voxel::Orientation::PositiveZ,
		voxel::Voxel::Flip::NegativeY), "slab_flipped");
}

TEST(VoxelMesherSemanticGoldenTest, SlopeMatchesCurrentBaker)
{
	expectSnapshot(bakeVolumeCell("debug_slope", {3, 1, 1}, voxel::Voxel::Orientation::PositiveX),
		"slope_positive_x");
}

TEST(VoxelMesherSemanticGoldenTest, StairMatchesCurrentBaker)
{
	expectSnapshot(bakeVolumeCell("debug_stair", {4, 1, 1}, voxel::Voxel::Orientation::NegativeX,
		voxel::Voxel::Flip::NegativeY), "stair_negative_x_flipped");
}

TEST(VoxelMesherSemanticGoldenTest, CrossMatchesCurrentBaker)
{
	expectSnapshot(bakeVolumeCell("plant", {5, 1, 1}, voxel::Voxel::Orientation::NegativeZ),
		"cross_negative_z");
}

TEST(VoxelMesherSemanticGoldenTest, MissingChunkMatchesCurrentBaker)
{
	expectSnapshot(bakeChunkBoundary(false), "boundary_unloaded_neighbor");
}

TEST(VoxelMesherSemanticGoldenTest, AdjacentChunkMatchesCurrentBaker)
{
	expectSnapshot(bakeChunkBoundary(true), "boundary_occluded_neighbor");
}
