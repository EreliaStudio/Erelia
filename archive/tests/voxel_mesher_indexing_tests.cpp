#include "voxel_test_utils.hpp"

#include "voxel/chunk_mesher.hpp"
#include "voxel/voxel_mesher.hpp"

#include <gtest/gtest.h>

#include <algorithm>
#include <iterator>
#include <span>
#include <string>
#include <string_view>
#include <utility>

#include <exception.hpp>

namespace
{
	void setFirstCell(voxel::VoxelVolume &volume,
		const voxel::Voxel::Catalog<voxel::Voxel::Definition> &catalog,
		std::string_view definition)
	{
		volume.edit().set({0, 0, 0}, voxel::Voxel::Cell(catalog.id(std::string(definition))));
	}

	template <typename TType>
	bool byteIdentical(std::span<const TType> left, std::span<const TType> right)
	{
		return std::ranges::equal(std::as_bytes(left), std::as_bytes(right));
	}

	bool byteIdentical(const spk::TextureMesh3D &left, const spk::TextureMesh3D &right)
	{
		const auto leftVertices = left.layout().vertexBuffer().cast<spk::Texture3DVertex>();
		const auto rightVertices = right.layout().vertexBuffer().cast<spk::Texture3DVertex>();
		const auto leftIndices = left.layout().indexBuffer().cast<spk::TextureMesh3D::Index>();
		const auto rightIndices = right.layout().indexBuffer().cast<spk::TextureMesh3D::Index>();
		return byteIdentical(leftVertices, rightVertices) && byteIdentical(leftIndices, rightIndices);
	}
}

TEST(VoxelMesherIndexingTest, ReusesExistingStairVerticesAtTheirFirstEncounter)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelVolume volume({1, 1, 1}, 1.0f);
	setFirstCell(volume, catalog, "debug_stair");
	const auto mesh = voxel::VoxelMesher(catalog).bake(volume);
	const auto indices = mesh.layout().indexBuffer().cast<spk::TextureMesh3D::Index>();

	ASSERT_EQ(mesh.vertexCount(), 38);
	ASSERT_EQ(mesh.indexCount(), 60);
	EXPECT_EQ(indices[25], indices[30]);
	EXPECT_EQ(indices[41], indices[42]);
}

TEST(VoxelMesherIndexingTest, KeepsDifferentHardNormalsAtTheSamePositionDistinct)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelVolume volume({1, 1, 1}, 1.0f);
	setFirstCell(volume, catalog, "stone");
	const auto mesh = voxel::VoxelMesher(catalog).bake(volume);
	const auto vertices = mesh.layout().vertexBuffer().cast<spk::Texture3DVertex>();
	const auto matchesPosition = [](const auto &vertex) {
		return vertex.position == spk::Vector3(0.0f, 0.0f, 0.0f);
	};

	EXPECT_EQ(mesh.vertexCount(), 24);
	EXPECT_EQ(std::ranges::count_if(vertices, matchesPosition), 3);
	EXPECT_TRUE(std::ranges::any_of(vertices, [&](const auto &vertex) {
		return matchesPosition(vertex) && vertex.normal == spk::Vector3(-1.0f, 0.0f, 0.0f);
	}));
	EXPECT_TRUE(std::ranges::any_of(vertices, [&](const auto &vertex) {
		return matchesPosition(vertex) && vertex.normal == spk::Vector3(0.0f, -1.0f, 0.0f);
	}));
	EXPECT_TRUE(std::ranges::any_of(vertices, [&](const auto &vertex) {
		return matchesPosition(vertex) && vertex.normal == spk::Vector3(0.0f, 0.0f, -1.0f);
	}));
}

TEST(VoxelMesherIndexingTest, KeepsAtlasUvSeamsDistinct)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelVolume volume({2, 1, 1}, 1.0f);
	setFirstCell(volume, catalog, "stone");
	volume.edit().set({1, 0, 0}, voxel::Voxel::Cell(catalog.id("stone")));
	const auto mesh = voxel::VoxelMesher(catalog).bake(volume);
	const auto vertices = mesh.layout().vertexBuffer().cast<spk::Texture3DVertex>();
	const auto seam = [](const auto &vertex) {
		return vertex.position == spk::Vector3(1.0f, 1.0f, 0.0f) &&
			vertex.normal == spk::Vector3(0.0f, 1.0f, 0.0f);
	};

	ASSERT_EQ(mesh.vertexCount(), 40);
	ASSERT_EQ(std::ranges::count_if(vertices, seam), 2);
	auto first = std::ranges::find_if(vertices, seam);
	auto second = std::ranges::find_if(std::next(first), vertices.end(), seam);
	EXPECT_NE(first->uv, second->uv);
}

TEST(VoxelMesherIndexingTest, ProducesByteIdenticalFirstSeenOutputForVolumeAndChunk)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelVolume volume({1, 1, 1}, 1.0f);
	setFirstCell(volume, catalog, "debug_stair");
	voxel::VoxelMesher mesher(catalog);
	const auto first = mesher.bake(volume);
	const auto second = mesher.bake(volume);
	voxel::Chunk::Collection chunks;
	auto chunk = playground_test::chunkWithCell(
		{0, 0, 0}, {0, 0, 0}, voxel::Voxel::Cell(catalog.id("debug_stair")));
	voxel::Chunk *source = chunk.get();
	ASSERT_TRUE(playground_test::publish(chunks, std::move(chunk)));
	const auto chunkMesh = voxel::Chunk::Mesher(catalog, chunks).bake(*source);

	EXPECT_TRUE(byteIdentical(first, second));
	EXPECT_TRUE(byteIdentical(first, chunkMesh));
}

TEST(VoxelMesherIndexingTest, RejectedDefinitionCannotPoisonLaterIndexing)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelVolume valid({1, 1, 1}, 1.0f);
	setFirstCell(valid, catalog, "debug_stair");
	voxel::VoxelVolume invalid({1, 1, 1}, 1.0f);
	invalid.edit().set({0, 0, 0}, voxel::Voxel::Cell(catalog.size() + 1));
	voxel::VoxelMesher mesher(catalog);
	const auto expected = mesher.bake(valid);

	EXPECT_THROW(static_cast<void>(mesher.bake(invalid)), spk::Exception);
	const auto recovered = mesher.bake(valid);
	EXPECT_TRUE(byteIdentical(expected, recovered));
}
