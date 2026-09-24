#include "voxel_test_utils.hpp"

#include <gtest/gtest.h>

#include <cmath>

namespace
{
	void expectConvex(const voxel::Voxel::Shape::Polygon &polygon)
	{
		ASSERT_GE(polygon.vertices.size(), 3);
		for (std::size_t index = 0; index < polygon.vertices.size(); ++index)
		{
			const auto &previous = polygon.vertices[index].position;
			const auto &current = polygon.vertices[(index + 1) % polygon.vertices.size()].position;
			const auto &next = polygon.vertices[(index + 2) % polygon.vertices.size()].position;
			EXPECT_GE((current - previous).cross(next - current).dot(polygon.normal), -0.0001f);
		}
	}
}

TEST(VoxelCatalogTest, LoadsCurrentDefinitions)
{
	const auto catalog = playground_test::loadVoxelCatalog();
	EXPECT_EQ(catalog.size(), 7);
	EXPECT_NE(catalog.id("grass"), 0);
}

TEST(VoxelCatalogTest, CharacterizesSlabTop)
{
	const auto catalog = playground_test::loadVoxelCatalog();
	bool foundTop = false;
	for (const auto &polygon : catalog.definition("debug_slab").shape->polygons)
	{
		if (polygon.outerSide == voxel::Voxel::Side::None && std::abs(polygon.normal.y) > 0.5f)
		{
			foundTop = true;
			EXPECT_GT(polygon.normal.y, 0.0f);
		}
	}
	EXPECT_TRUE(foundTop);
}

TEST(VoxelCatalogTest, CharacterizesSlopeTop)
{
	const auto catalog = playground_test::loadVoxelCatalog();
	bool foundTop = false;
	for (const auto &polygon : catalog.definition("debug_slope").shape->polygons)
	{
		if (polygon.outerSide == voxel::Voxel::Side::None && std::abs(polygon.normal.y) > 0.5f && std::abs(polygon.normal.z) > 0.5f)
		{
			foundTop = true;
			EXPECT_GT(polygon.normal.y, 0.0f);
			EXPECT_LT(polygon.normal.z, 0.0f);
		}
	}
	EXPECT_TRUE(foundTop);
}

TEST(VoxelCatalogTest, CharacterizesConvexStairTreadsAndRisers)
{
	const auto catalog = playground_test::loadVoxelCatalog();
	bool foundRiser = false;
	bool foundLowerTread = false;
	for (const auto &polygon : catalog.definition("debug_stair").shape->polygons)
	{
		expectConvex(polygon);
		if (polygon.outerSide == voxel::Voxel::Side::None && std::abs(polygon.normal.y) > 0.5f)
		{
			foundLowerTread = true;
			EXPECT_GT(polygon.normal.y, 0.0f);
		}
		if (polygon.outerSide == voxel::Voxel::Side::None && std::abs(polygon.normal.z) > 0.5f)
		{
			foundRiser = true;
			EXPECT_LT(polygon.normal.z, 0.0f);
		}
	}
	EXPECT_TRUE(foundLowerTread);
	EXPECT_TRUE(foundRiser);
}
