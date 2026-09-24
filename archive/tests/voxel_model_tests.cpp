#include "voxel/voxel_model.hpp"

#include <array>
#include <limits>
#include <type_traits>

#include <exception.hpp>
#include <gtest/gtest.h>

TEST(VoxelModelTest, SupportsRepresentativeRuntimeDimensionsWithOneConcreteType)
{
	static_assert(std::is_final_v<voxel::VoxelModel>);
	static_assert(std::is_base_of_v<voxel::VoxelVolume, voxel::VoxelModel>);
	const std::array<spk::Vector3UInt, 3> dimensions = {
		spk::Vector3UInt{10, 10, 14},
		spk::Vector3UInt{12, 7, 14},
		spk::Vector3UInt{32, 32, 22}};

	for (const auto expected : dimensions)
	{
		const voxel::VoxelModel model(expected, 0.1f);
		EXPECT_EQ(model.dimensions(), expected);
	}
}

TEST(VoxelModelTest, ReportsBoundsFromRuntimeDimensionsAndUniformScale)
{
	const voxel::VoxelModel model({8, 8, 16}, 0.1f);
	const auto bounds = model.localBounds();

	EXPECT_EQ(bounds.minimum, spk::Vector3());
	EXPECT_FLOAT_EQ(bounds.maximum.x, 0.8f);
	EXPECT_FLOAT_EQ(bounds.maximum.y, 0.8f);
	EXPECT_FLOAT_EQ(bounds.maximum.z, 1.6f);
}

TEST(VoxelModelTest, UsesInheritedPackedCellEditorAndSingleInvalidation)
{
	voxel::VoxelModel model({2, 2, 2}, 0.1f);
	const voxel::Voxel::Cell cell(17, voxel::Voxel::Orientation::NegativeX, voxel::Voxel::Flip::NegativeY);
	int notifications = 0;
	auto contract = model.subscribeToVersionEdition([&](spk::VersionedTrait *versioned) {
		++notifications;
		EXPECT_EQ(versioned, &model);
	});
	{
		auto editor = model.edit();
		EXPECT_TRUE(editor.set({0, 0, 0}, cell));
		EXPECT_TRUE(editor.set({1, 1, 1}, voxel::Voxel::Cell(23)));
	}

	EXPECT_EQ(model.at({0, 0, 0}).packed(), cell.packed());
	EXPECT_EQ(model.at({1, 1, 1}), voxel::Voxel::Cell(23));
	EXPECT_EQ(model.version(), spk::VersionedTrait::Version{1});
	EXPECT_EQ(notifications, 1);
}

TEST(VoxelModelTest, UsesInheritedValidationPolicy)
{
	EXPECT_THROW(voxel::VoxelModel({0, 1, 1}, 0.1f), spk::Exception);
	EXPECT_THROW(voxel::VoxelModel({1, 1, 1}, 0.0f), spk::Exception);
	EXPECT_THROW(voxel::VoxelModel({1, 1, 1}, std::numeric_limits<float>::infinity()), spk::Exception);

	voxel::VoxelModel model({1, 1, 1}, 0.1f);
	EXPECT_THROW(static_cast<void>(model.at({1, 0, 0})), spk::Exception);
	EXPECT_THROW(model.edit().set({-1, 0, 0}, voxel::Voxel::Cell(1)), spk::Exception);
	EXPECT_EQ(model.version(), spk::VersionedTrait::Version{0});
}
