#include "voxel/voxel_volume.hpp"

#include <cstdint>
#include <limits>
#include <string>

#include <exception.hpp>
#include <gtest/gtest.h>

TEST(VoxelVolumeMetadataTest, ExposesDimensionsScaleAndDerivedLocalBounds)
{
	const voxel::VoxelVolume volume({8, 8, 16}, 0.1f);
	const auto bounds = volume.localBounds();

	EXPECT_EQ(volume.dimensions(), spk::Vector3UInt(8, 8, 16));
	EXPECT_FLOAT_EQ(volume.voxelSize(), 0.1f);
	EXPECT_EQ(volume.cells().size(), 1024);
	EXPECT_TRUE(volume.at({0, 0, 0}).empty());
	EXPECT_EQ(bounds.minimum, spk::Vector3());
	EXPECT_FLOAT_EQ(bounds.maximum.x, 0.8f);
	EXPECT_FLOAT_EQ(bounds.maximum.y, 0.8f);
	EXPECT_FLOAT_EQ(bounds.maximum.z, 1.6f);
}

TEST(VoxelVolumeMetadataTest, RejectsEmptyDimensions)
{
	EXPECT_THROW(voxel::VoxelVolume({0, 1, 1}, 1.0f), spk::Exception);
	EXPECT_THROW(voxel::VoxelVolume({1, 0, 1}, 1.0f), spk::Exception);
	EXPECT_THROW(voxel::VoxelVolume({1, 1, 0}, 1.0f), spk::Exception);
}

TEST(VoxelVolumeMetadataTest, RejectsNonPositiveOrNonFiniteVoxelSize)
{
	EXPECT_THROW(voxel::VoxelVolume({1, 1, 1}, 0.0f), spk::Exception);
	EXPECT_THROW(voxel::VoxelVolume({1, 1, 1}, -0.1f), spk::Exception);
	EXPECT_THROW(voxel::VoxelVolume({1, 1, 1}, std::numeric_limits<float>::infinity()), spk::Exception);
	EXPECT_THROW(voxel::VoxelVolume({1, 1, 1}, std::numeric_limits<float>::quiet_NaN()), spk::Exception);
}

TEST(VoxelVolumeMetadataTest, RejectsUnrepresentableCellCount)
{
	constexpr auto maximum = std::numeric_limits<std::uint32_t>::max();
	EXPECT_THROW(voxel::VoxelVolume({maximum, maximum, maximum}, 1.0f), spk::Exception);
}

TEST(VoxelVolumeMetadataTest, ReportsStableMessageAndThrowSite)
{
	try
	{
		voxel::VoxelVolume volume({0, 1, 1}, 1.0f);
		FAIL() << "expected spk::Exception";
	} catch (const spk::Exception &exception)
	{
		EXPECT_EQ(exception.message(), "voxel volume dimensions must be positive");
		EXPECT_NE(exception.location().line(), 0u);
		EXPECT_FALSE(std::string(exception.location().file_name()).empty());
		EXPECT_EQ(exception.cause(), nullptr);
	}
}
