#include "voxel/voxel_transform.hpp"

#include <gtest/gtest.h>

TEST(VoxelShapeTransformTest, PreservesMaterialAndUvWhileApplyingOrientationAndFlip)
{
	voxel::Voxel::Shape::Polygon polygon{
		.vertices = {{{0.0f, 0.25f, 0.0f}, {0.125f, 0.75f}}, {{1.0f, 0.25f, 0.0f}, {0.875f, 0.75f}}, {{1.0f, 0.75f, 0.0f}, {0.875f, 0.25f}}},
		.materialSlot = "side",
		.normal = {0.0f, 0.0f, -1.0f},
		.outerSide = voxel::Voxel::Side::NegativeZ};
	const voxel::Voxel::Cell transform(1, voxel::Voxel::Orientation::PositiveX, voxel::Voxel::Flip::NegativeY);
	const auto result = voxel::Voxel::transformPolygon(polygon, transform);
	EXPECT_EQ(result.materialSlot, "side");
	EXPECT_EQ(result.vertices[0].uv, spk::Vector2(0.125f, 0.75f));
	EXPECT_EQ(result.vertices[0].position, spk::Vector3(0.0f, 0.75f, 1.0f));
	EXPECT_EQ(result.vertices[2].position, spk::Vector3(0.0f, 0.25f, 0.0f));
	EXPECT_EQ(result.normal, spk::Vector3(-1.0f, 0.0f, 0.0f));
	EXPECT_EQ(result.outerSide, voxel::Voxel::Side::NegativeX);
}
