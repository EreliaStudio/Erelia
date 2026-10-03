#include "erelia/core/world.hpp"
#include "erelia/core/world_service.hpp"

#include <engine/entity3d.hpp>
#include <gtest/gtest.h>

#include <utility>

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
		explicit TestWorld(WorldIdentifier identifier) :
			World(std::move(identifier))
		{
		}

		std::size_t chunkCreations = 0;
		std::size_t columnCreations = 0;
	};

	class TestWorldProvider final : public WorldProvider
	{
	protected:
		[[nodiscard]] std::unique_ptr<World> _acquire(
			const WorldIdentifier &identifier) override
		{
			++acquisitions;
			return std::make_unique<TestWorld>(
				identifier);
		}

	public:
		std::size_t acquisitions = 0;
	};

	class MismatchedWorldProvider final : public WorldProvider
	{
	protected:
		[[nodiscard]] std::unique_ptr<World> _acquire(
			const WorldIdentifier &) override
		{
			return std::make_unique<TestWorld>(
				WorldIdentifier{
					.name = "other"});
		}
	};
}

TEST(World, LazilyCreatesStableCollectionPointers)
{
	TestWorld world(
		WorldIdentifier{
			.name = "test.world"});

	EXPECT_EQ(world.identifier().name, "test.world");
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
	TestWorld world(
		WorldIdentifier{
			.name = "test.world"});

	spk::Entity3D *entity =
		world.addEntity<spk::Entity3D>("Entity");

	ASSERT_NE(entity, nullptr);
	ASSERT_EQ(world.engine().root().children().size(), 1u);
	EXPECT_EQ(world.engine().root().children().front(), entity);
}

TEST(WorldService, LazilyAcquiresAndOwnsNamedWorlds)
{
	WorldService worlds(
		TestWorldProvider{});

	const WorldIdentifier levelIdentifier{
		.name = "world.level1"};
	const WorldIdentifier hubIdentifier{
		.name = "spawn.hub"};

	World *level = worlds.world(levelIdentifier);
	World *sameLevel = worlds.world(levelIdentifier);
	World *hub = worlds.world(hubIdentifier);

	ASSERT_NE(level, nullptr);
	ASSERT_NE(hub, nullptr);
	EXPECT_EQ(level, sameLevel);
	EXPECT_NE(level, hub);
	EXPECT_EQ(level->identifier(), levelIdentifier);
	EXPECT_EQ(hub->identifier(), hubIdentifier);
	EXPECT_EQ(
		static_cast<TestWorldProvider &>(
			worlds.provider())
			.acquisitions,
		2u);
	EXPECT_EQ(worlds.size(), 2u);
	EXPECT_EQ(worlds.find(levelIdentifier), level);
	EXPECT_EQ(worlds.find(hubIdentifier), hub);

	EXPECT_EQ(worlds.remove(levelIdentifier), true);
	EXPECT_EQ(worlds.find(levelIdentifier), nullptr);
	EXPECT_EQ(worlds.size(), 1u);

	World *recreated =
		worlds.world(levelIdentifier);
	ASSERT_NE(recreated, nullptr);
	EXPECT_EQ(recreated->identifier(), levelIdentifier);
	EXPECT_EQ(
		static_cast<TestWorldProvider &>(
			worlds.provider())
			.acquisitions,
		3u);

	worlds.clear();
	EXPECT_EQ(worlds.size(), 0u);
}

TEST(WorldService, RejectsProviderIdentifierMismatch)
{
	WorldService worlds(
		MismatchedWorldProvider{});

	EXPECT_THROW(
		(void)worlds.world(
			WorldIdentifier{
				.name = "requested"}),
		spk::Exception);
	EXPECT_EQ(worlds.size(), 0u);
}
