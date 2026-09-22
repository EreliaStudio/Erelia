#include "semantic_mesh_snapshot.hpp"
#include "voxel_mesher_fixture.hpp"
#include "voxel_test_utils.hpp"

#include "voxel/chunk_mesher.hpp"
#include "voxel/voxel_mesher.hpp"

#include <gtest/gtest.h>

#include <algorithm>
#include <array>
#include <cmath>

#include <exception.hpp>

namespace
{
	class ProbeVoxelMesher final : public voxel::VoxelMesher
	{
	public:
		using VoxelMesher::VoxelMesher;

		[[nodiscard]] CacheStatistics statistics() const noexcept { return _cacheStatistics(); }
	};

	std::size_t occupiedCellCount(const voxel::VoxelVolume &volume)
	{
		return static_cast<std::size_t>(std::ranges::count_if(
			volume.cells(), [](voxel::Voxel::Cell cell) { return !cell.empty(); }));
	}

	std::unique_ptr<voxel::Chunk> filledBoundaryChunk(
		voxel::Chunk::Coordinate chunkCoordinate, int firstX)
	{
		auto chunk = std::make_unique<voxel::Chunk>(chunkCoordinate);
		auto editor = chunk->edit();
		for (int x = firstX; x < firstX + 2; ++x)
			for (int y = 2; y < 4; ++y)
				for (int z = 4; z < 6; ++z)
					editor.set({x, y, z}, voxel::Voxel::Cell(3));
		return chunk;
	}

	struct BoundaryCase
	{
		voxel::Chunk::Coordinate neighborChunk;
		spk::Vector3Int sourceCell;
		spk::Vector3Int neighborCell;
	};

	const std::array<BoundaryCase, 6> BoundaryCases{{
		{{-1, 0, 0}, {0, 2, 3}, {15, 2, 3}}, {{1, 0, 0}, {15, 2, 3}, {0, 2, 3}},
		{{0, -1, 0}, {2, 0, 3}, {2, 15, 3}}, {{0, 1, 0}, {2, 15, 3}, {2, 0, 3}},
		{{0, 0, -1}, {2, 3, 0}, {2, 3, 15}}, {{0, 0, 1}, {2, 3, 15}, {2, 3, 0}}}};
}

TEST(VoxelMesherTest, LoadsJsonModelAndMatchesProceduralRawVolume)
{
	auto catalog = playground_test::loadVoxelCatalog();
	auto model = playground_test::loadVoxelMesherModel();
	auto volume = playground_test::makeVoxelMesherVolume();
	voxel::VoxelMesher mesher(catalog);
	const auto modelMesh = mesher.bake(*model);
	const auto volumeMesh = mesher.bake(*volume);

	EXPECT_EQ(model->dimensions(), spk::Vector3UInt(8, 8, 16));
	EXPECT_FLOAT_EQ(model->voxelSize(), 0.1f);
	EXPECT_EQ(occupiedCellCount(*model), playground_test::MesherFixtureCellCount);
	EXPECT_EQ(modelMesh.layout().vertexBuffer().cast<spk::Texture3DVertex>().size(),
		playground_test::MesherFixtureVertexCount);
	EXPECT_EQ(modelMesh.indexCount(), playground_test::MesherFixtureIndexCount);
	EXPECT_EQ(playground_test::semanticMeshSnapshot(modelMesh),
		playground_test::semanticMeshSnapshot(volumeMesh));
}

TEST(VoxelMesherTest, CullsCrossStatueInteriorAndReusesOcclusionCache)
{
	auto catalog = playground_test::loadVoxelCatalog();
	auto volume = playground_test::makeVoxelMesherVolume();
	ProbeVoxelMesher mesher(catalog);
	const auto first = mesher.bake(*volume);
	const auto firstStatistics = mesher.statistics();
	const auto second = mesher.bake(*volume);
	const auto secondStatistics = mesher.statistics();

	EXPECT_EQ(first.indexCount(), 432);
	EXPECT_GT(firstStatistics.entries, 0);
	EXPECT_EQ(secondStatistics.entries, firstStatistics.entries);
	EXPECT_EQ(secondStatistics.misses, firstStatistics.misses);
	EXPECT_GT(secondStatistics.hits, firstStatistics.hits);
	EXPECT_EQ(playground_test::semanticMeshSnapshot(first), playground_test::semanticMeshSnapshot(second));
}

TEST(VoxelMesherTest, UniformScaleChangesPositionsOnly)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelVolume unit({2, 1, 1}, 1.0f);
	voxel::VoxelVolume scaled({2, 1, 1}, 0.1f);
	{
		auto editor = unit.edit();
		editor.set({0, 0, 0}, voxel::Voxel::Cell(catalog.id("stone")));
		editor.set({1, 0, 0}, voxel::Voxel::Cell(catalog.id("stone")));
	}
	{
		auto editor = scaled.edit();
		editor.set({0, 0, 0}, voxel::Voxel::Cell(catalog.id("stone")));
		editor.set({1, 0, 0}, voxel::Voxel::Cell(catalog.id("stone")));
	}
	voxel::VoxelMesher mesher(catalog);
	const auto unitMesh = mesher.bake(unit);
	const auto scaledMesh = mesher.bake(scaled);
	const auto unitVertices = unitMesh.layout().vertexBuffer().cast<spk::Texture3DVertex>();
	const auto scaledVertices = scaledMesh.layout().vertexBuffer().cast<spk::Texture3DVertex>();

	ASSERT_EQ(unitVertices.size(), 40);
	ASSERT_EQ(unitVertices.size(), scaledVertices.size());
	EXPECT_EQ(unitMesh.indexCount(), 60);
	EXPECT_EQ(unitMesh.indexCount(), scaledMesh.indexCount());
	for (std::size_t index = 0; index < unitVertices.size(); ++index)
	{
		EXPECT_EQ(unitVertices[index].normal, scaledVertices[index].normal);
		EXPECT_EQ(unitVertices[index].uv, scaledVertices[index].uv);
		EXPECT_FLOAT_EQ(unitVertices[index].position.x * 0.1f, scaledVertices[index].position.x);
		EXPECT_FLOAT_EQ(unitVertices[index].position.y * 0.1f, scaledVertices[index].position.y);
		EXPECT_FLOAT_EQ(unitVertices[index].position.z * 0.1f, scaledVertices[index].position.z);
		EXPECT_NEAR(scaledVertices[index].normal.dot(scaledVertices[index].normal), 1.0f, 0.0001f);
	}
}

TEST(VoxelMesherTest, StandaloneBoundaryIsEmpty)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelVolume volume({1, 1, 1}, 0.1f);
	volume.edit().set({0, 0, 0}, voxel::Voxel::Cell(catalog.id("stone")));
	const auto mesh = voxel::VoxelMesher(catalog).bake(volume);
	EXPECT_EQ(mesh.layout().vertexBuffer().cast<spk::Texture3DVertex>().size(), 24);
	EXPECT_EQ(mesh.indexCount(), 36);
}

TEST(VoxelMesherTest, RejectedDefinitionDoesNotChangeCacheOrLaterOutput)
{
	auto catalog = playground_test::loadVoxelCatalog();
	auto valid = playground_test::makeVoxelMesherVolume();
	voxel::VoxelVolume invalid({1, 1, 1}, 1.0f);
	invalid.edit().set({0, 0, 0}, voxel::Voxel::Cell(catalog.size() + 1));
	ProbeVoxelMesher mesher(catalog);
	const auto expected = playground_test::semanticMeshSnapshot(mesher.bake(*valid));
	const auto statistics = mesher.statistics();

	EXPECT_THROW(static_cast<void>(mesher.bake(invalid)), spk::Exception);
	EXPECT_EQ(mesher.statistics(), statistics);
	EXPECT_EQ(playground_test::semanticMeshSnapshot(mesher.bake(*valid)), expected);
}

TEST(ChunkMesherTest, FilledAdjacentSectionsOccludeAcrossChunkBoundary)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::Chunk::Collection isolated;
	auto isolatedLeft = filledBoundaryChunk({0, 0, 0}, 14);
	voxel::Chunk *isolatedSource = isolatedLeft.get();
	ASSERT_TRUE(playground_test::publish(isolated, std::move(isolatedLeft)));
	EXPECT_EQ(voxel::Chunk::Mesher(catalog, isolated).bake(*isolatedSource).indexCount(), 144);

	voxel::Chunk::Collection adjacent;
	auto left = filledBoundaryChunk({0, 0, 0}, 14);
	auto right = filledBoundaryChunk({1, 0, 0}, 0);
	voxel::Chunk *leftSource = left.get();
	voxel::Chunk *rightSource = right.get();
	ASSERT_TRUE(playground_test::publish(adjacent, std::move(left)));
	ASSERT_TRUE(playground_test::publish(adjacent, std::move(right)));
	voxel::Chunk::Mesher mesher(catalog, adjacent);
	EXPECT_EQ(mesher.bake(*leftSource).indexCount(), 120);
	EXPECT_EQ(mesher.bake(*rightSource).indexCount(), 120);
}

TEST(ChunkMesherTest, ResolvesAllSixExternalNeighborDirections)
{
	auto catalog = playground_test::loadVoxelCatalog();
	for (const auto &fixture : BoundaryCases)
	{
		voxel::Chunk::Collection chunks;
		auto source = playground_test::chunkWithCell({0, 0, 0}, fixture.sourceCell, voxel::Voxel::Cell(3));
		auto neighbor = playground_test::chunkWithCell(fixture.neighborChunk, fixture.neighborCell, voxel::Voxel::Cell(3));
		voxel::Chunk *sourceChunk = source.get();
		voxel::Chunk *neighborChunk = neighbor.get();
		ASSERT_TRUE(playground_test::publish(chunks, std::move(source)));
		ASSERT_TRUE(playground_test::publish(chunks, std::move(neighbor)));
		voxel::Chunk::Mesher mesher(catalog, chunks);
		EXPECT_EQ(mesher.bake(*sourceChunk).indexCount(), 30);
		EXPECT_EQ(mesher.bake(*neighborChunk).indexCount(), 30);
	}
}
