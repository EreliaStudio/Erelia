#include "erelia/core/world.hpp"
#include "erelia/core/world_collection.hpp"

#include <engine/entity3d.hpp>
#include <gtest/gtest.h>

namespace
{
	class ChunkProvider final : public World::Chunks::Provider
	{
	protected:
		[[nodiscard]] World::Chunks::Answer _acquire(
			const Chunk::Coordinate &) override
		{
			spk::Task<Chunk> task;
			task.validate(Chunk{});
			return task.answer();
		}
	};

	class ColumnProvider final : public World::Columns::Provider
	{
	protected:
		[[nodiscard]] World::Columns::Answer _acquire(
			const Column::Coordinate &) override
		{
			spk::Task<Column> task;
			task.validate(Column{});
			return task.answer();
		}
	};

	class TestWorld final : public World
	{
	private:
		std::unique_ptr<Chunks> _createChunkCollection() override
		{
			++chunkCreations;
			return std::make_unique<Chunks>(ChunkProvider{});
		}

		std::unique_ptr<Columns> _createColumnCollection() override
		{
			++columnCreations;
			return std::make_unique<Columns>(ColumnProvider{});
		}

	public:
		std::size_t chunkCreations = 0;
		std::size_t columnCreations = 0;
	};

	class TestWorldCollection final : public WorldCollection
	{
	private:
		[[nodiscard]] std::unique_ptr<World> _createWorld(
			const Identifier &) override
		{
			++creations;
			return std::make_unique<TestWorld>();
		}

	public:
		std::size_t creations = 0;
	};
}

TEST(World, LazilyCreatesStableCollectionPointers)
{
	TestWorld world;

	EXPECT_EQ(world.chunkCreations, 0u);
	EXPECT_EQ(world.columnCreations, 0u);

	World::Chunks *chunks = world.chunkCollection();
	ASSERT_NE(chunks, nullptr);
	EXPECT_EQ(world.chunkCreations, 1u);
	EXPECT_EQ(world.columnCreations, 0u);
	EXPECT_EQ(world.chunkCollection(), chunks);
	EXPECT_EQ(world.chunkCreations, 1u);

	World::Columns *columns = world.columnCollection();
	ASSERT_NE(columns, nullptr);
	EXPECT_EQ(world.columnCreations, 1u);
	EXPECT_EQ(world.columnCollection(), columns);
	EXPECT_EQ(world.columnCreations, 1u);
}

TEST(World, OwnsEntitiesAddedToItsEngine)
{
	TestWorld world;

	spk::Entity3D *entity =
		world.addEntity<spk::Entity3D>("Entity");

	ASSERT_NE(entity, nullptr);
	ASSERT_EQ(world.engine().root().children().size(), 1u);
	EXPECT_EQ(world.engine().root().children().front(), entity);
}

TEST(WorldCollection, LazilyCreatesAndOwnsNamedWorlds)
{
	TestWorldCollection worlds;

	World *level = worlds.world("world.level1");
	World *sameLevel = worlds.world("world.level1");
	World *hub = worlds.world("spawn.hub");

	ASSERT_NE(level, nullptr);
	ASSERT_NE(hub, nullptr);
	EXPECT_EQ(level, sameLevel);
	EXPECT_NE(level, hub);
	EXPECT_EQ(worlds.creations, 2u);
	EXPECT_EQ(worlds.size(), 2u);
	EXPECT_EQ(worlds.find("world.level1"), level);
	EXPECT_EQ(worlds.find("spawn.hub"), hub);
	EXPECT_EQ(worlds.find("missing"), nullptr);

	EXPECT_EQ(worlds.remove("world.level1"), true);
	EXPECT_EQ(worlds.find("world.level1"), nullptr);
	EXPECT_EQ(worlds.size(), 1u);

	World *recreated = worlds.world("world.level1");
	ASSERT_NE(recreated, nullptr);
	EXPECT_EQ(worlds.creations, 3u);
	EXPECT_EQ(worlds.size(), 2u);

	worlds.clear();
	EXPECT_EQ(worlds.size(), 0u);
}
