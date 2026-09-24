#include "voxel/voxel_cell.hpp"

#include <gtest/gtest.h>

TEST(VoxelCellTest, PreservesPackedDefinitionOrientationAndFlip)
{
	using voxel::Voxel::Cell;
	static_assert(sizeof(Cell) == sizeof(std::uint32_t));
	const Cell transformed(42, voxel::Voxel::Orientation::NegativeX, voxel::Voxel::Flip::NegativeY);
	EXPECT_EQ(transformed.packed(), 0xE000002Au);
	EXPECT_EQ(Cell::fromPacked(0xE000002Au), transformed);
	EXPECT_TRUE(Cell{}.empty());
	EXPECT_EQ(Cell{}.packed(), 0);
	EXPECT_THROW(static_cast<void>(Cell(Cell::IDMask + 1)), spk::Exception);
}
