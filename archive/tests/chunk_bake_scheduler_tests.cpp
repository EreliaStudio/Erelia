#include "voxel_test_utils.hpp"

#include "voxel/chunk_bake_scheduler.hpp"

#include <array>
#include <exception.hpp>
#include <gtest/gtest.h>
#include <set>
#include <vector>

namespace
{
	constexpr std::array<voxel::Chunk::Coordinate, 7> Neighborhood{{
		{0, 0, 0}, {-1, 0, 0}, {1, 0, 0}, {0, -1, 0}, {0, 1, 0}, {0, 0, -1}, {0, 0, 1}}};

	void publishNeighborhood(voxel::Chunk::Collection &chunks)
	{
		for (const auto coordinate : Neighborhood)
			ASSERT_TRUE(playground_test::publish(chunks, std::make_unique<voxel::Chunk>(coordinate)));
	}
}

TEST(ChunkBakeSchedulerTest, NotifiesEverySubscriberForInitialBakeAndEdit)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::Chunk::Collection chunks;
	voxel::Chunk::Mesher mesher(catalog, chunks);
	voxel::Chunk::BakeScheduler scheduler(chunks, mesher);
	std::vector<std::size_t> firstMeshes;
	int secondConsumer = 0;
	auto firstBake = scheduler.subscribeToBakeCompletion(
		[&](voxel::Chunk::Coordinate, const spk::TextureMesh3D &mesh) { firstMeshes.push_back(mesh.indexCount()); });
	auto secondBake = scheduler.subscribeToBakeCompletion(
		[&](voxel::Chunk::Coordinate, const spk::TextureMesh3D &) { ++secondConsumer; });
	ASSERT_TRUE(playground_test::publish(chunks, std::make_unique<voxel::Chunk>(voxel::Chunk::Coordinate{0, 0, 0})));
	scheduler.process();
	EXPECT_EQ(firstMeshes, std::vector<std::size_t>{0});
	EXPECT_EQ(secondConsumer, 1);

	chunks.find({0, 0, 0})->edit().set({1, 1, 1}, voxel::Voxel::Cell(catalog.id("stone")));
	EXPECT_EQ(scheduler.pendingCount(), 1);
	scheduler.process();
	EXPECT_EQ(firstMeshes, (std::vector<std::size_t>{0, 36}));
	EXPECT_EQ(secondConsumer, 2);
}

TEST(ChunkBakeSchedulerTest, RebuildsEditedChunkAndEveryAvailableFaceNeighbor)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::Chunk::Collection chunks;
	voxel::Chunk::Mesher mesher(catalog, chunks);
	voxel::Chunk::BakeScheduler scheduler(chunks, mesher);
	std::set<voxel::Chunk::Coordinate> rebuilt;
	auto completion = scheduler.subscribeToBakeCompletion(
		[&](voxel::Chunk::Coordinate coordinate, const spk::TextureMesh3D &) { rebuilt.insert(coordinate); });
	publishNeighborhood(chunks);
	scheduler.process();
	rebuilt.clear();

	chunks.find({0, 0, 0})->edit().set({1, 1, 1}, voxel::Voxel::Cell(catalog.id("stone")));
	EXPECT_EQ(scheduler.pendingCount(), Neighborhood.size());
	scheduler.process();
	EXPECT_EQ(rebuilt, std::set<voxel::Chunk::Coordinate>(Neighborhood.begin(), Neighborhood.end()));
}

TEST(ChunkBakeSchedulerTest, InvalidDefinitionDoesNotPoisonLaterSchedulingOrBaking)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::Chunk::Collection chunks;
	voxel::Chunk::Mesher mesher(catalog, chunks);
	voxel::Chunk::BakeScheduler scheduler(chunks, mesher);
	std::set<voxel::Chunk::Coordinate> rebuilt;
	auto completion = scheduler.subscribeToBakeCompletion(
		[&](voxel::Chunk::Coordinate coordinate, const spk::TextureMesh3D &) { rebuilt.insert(coordinate); });
	ASSERT_TRUE(playground_test::publish(chunks, playground_test::chunkWithCell(
		{0, 0, 0}, {1, 1, 1}, voxel::Voxel::Cell(catalog.size() + 1))));
	ASSERT_TRUE(playground_test::publish(chunks, playground_test::chunkWithCell(
		{1, 0, 0}, {1, 1, 1}, voxel::Voxel::Cell(catalog.id("stone")))));

	EXPECT_THROW(static_cast<void>(scheduler.process()), spk::Exception);
	EXPECT_TRUE(rebuilt.empty());
	EXPECT_EQ(scheduler.pendingCount(), 1);
	chunks.find({0, 0, 0})->edit().set({1, 1, 1}, voxel::Voxel::Cell(catalog.id("stone")));
	EXPECT_EQ(scheduler.pendingCount(), 2);
	EXPECT_EQ(scheduler.process(), 2);
	EXPECT_EQ(rebuilt, (std::set<voxel::Chunk::Coordinate>{{0, 0, 0}, {1, 0, 0}}));
}
