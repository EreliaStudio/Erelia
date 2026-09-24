#include "erelia/core/voxel/catalog.hpp"

#include "voxel_resource_test_utils.hpp"

#include <gtest/gtest.h>

#include <type_traits>
#include <utility>

static_assert(std::is_copy_constructible_v<Voxel::Definition>);
static_assert(std::is_move_constructible_v<Voxel::Definition>);
static_assert(!std::is_copy_assignable_v<Voxel::Definition>);
static_assert(!std::is_move_assignable_v<Voxel::Definition>);

static_assert(!std::is_copy_constructible_v<Voxel::Catalog>);
static_assert(!std::is_move_constructible_v<Voxel::Catalog>);

namespace
{
	const std::string CompleteCubeSlots = R"({"top":"grass","side":"dirt","bottom":"stone"})";
}

TEST(VoxelDefinition, AirReferencesCatalogOwnedEmptyShape)
{
	Voxel::Catalog catalog;
	const Voxel::Definition &air = catalog.definitions().at(0u);

	EXPECT_TRUE(air.shape().polygons().empty());
	EXPECT_TRUE(air.slots().empty());
	EXPECT_FALSE(catalog.shapes().contains(""));
}

TEST(VoxelDefinition, LoadedDefinitionReferencesExactCatalogShape)
{
	const voxel_test::TemporaryJsonFile definitions(
		voxel_test::definitionFile(1u, "cube", CompleteCubeSlots));

	Voxel::Catalog catalog;
	catalog.load(voxel_test::shapeResourcePath(), definitions.path());

	const Voxel::Definition &definition = catalog.definitions().at(1u);
	EXPECT_EQ(&definition.shape(), &catalog.shapes().at("cube"));
	EXPECT_EQ(definition.slots().at("top"), "grass");
	EXPECT_EQ(definition.slots().at("side"), "dirt");
	EXPECT_EQ(definition.slots().at("bottom"), "stone");

	const Voxel::Definition &air = catalog.definitions().at(0u);
	EXPECT_NE(&air.shape(), &definition.shape());
	EXPECT_TRUE(air.shape().polygons().empty());
}

TEST(VoxelDefinition, CopyConstructionPreservesShapeReferenceAndBindings)
{
	const voxel_test::TemporaryJsonFile definitions(
		voxel_test::definitionFile(1u, "cube", CompleteCubeSlots));

	Voxel::Catalog catalog;
	catalog.load(voxel_test::shapeResourcePath(), definitions.path());

	const Voxel::Definition &source = catalog.definitions().at(1u);
	const Voxel::Definition copy(source);

	EXPECT_EQ(&copy.shape(), &source.shape());
	EXPECT_EQ(copy.slots(), source.slots());
}

TEST(VoxelDefinition, MoveConstructionPreservesShapeReferenceAndBindings)
{
	const voxel_test::TemporaryJsonFile definitions(
		voxel_test::definitionFile(1u, "cube", CompleteCubeSlots));

	Voxel::Catalog catalog;
	catalog.load(voxel_test::shapeResourcePath(), definitions.path());

	Voxel::Definition value(catalog.definitions().at(1u));
	const Voxel::Shape *shape = &value.shape();
	const auto slots = value.slots();

	Voxel::Definition moved(std::move(value));

	EXPECT_EQ(&moved.shape(), shape);
	EXPECT_EQ(moved.slots(), slots);
}
