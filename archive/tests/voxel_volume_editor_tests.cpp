#include "voxel/voxel_volume.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

TEST(VoxelVolumeEditorTest, PublishesOneVersionForChangedEdit)
{
	voxel::VoxelVolume volume({2, 2, 2}, 1.0f);
	int notifications = 0;
	auto contract = volume.subscribeToVersionEdition([&](spk::VersionedTrait *versioned) {
		++notifications;
		EXPECT_EQ(versioned, &volume);
	});
	{
		auto editor = volume.edit();
		EXPECT_TRUE(editor.set({0, 0, 0}, voxel::Voxel::Cell(7)));
		EXPECT_FALSE(editor.set({0, 0, 0}, voxel::Voxel::Cell(7)));
		EXPECT_TRUE(editor.set({1, 1, 1}, voxel::Voxel::Cell(9)));
	}
	EXPECT_EQ(volume.version(), spk::VersionedTrait::Version{1});
	EXPECT_EQ(notifications, 1);
}

TEST(VoxelVolumeEditorTest, DoesNotPublishVersionForNoOpEdit)
{
	voxel::VoxelVolume volume({1, 1, 1}, 1.0f);
	volume.edit().set({0, 0, 0}, voxel::Voxel::Cell(7));
	const auto initialVersion = volume.version();
	int notifications = 0;
	auto contract = volume.subscribeToVersionEdition([&](spk::VersionedTrait *) { ++notifications; });

	EXPECT_FALSE(volume.edit().set({0, 0, 0}, voxel::Voxel::Cell(7)));
	EXPECT_EQ(volume.version(), initialVersion);
	EXPECT_EQ(notifications, 0);
}

TEST(VoxelVolumeEditorTest, RejectsOutOfRangeMutationWithoutAliasingOrNotification)
{
	voxel::VoxelVolume volume({2, 1, 1}, 1.0f);
	int notifications = 0;
	auto contract = volume.subscribeToVersionEdition([&](spk::VersionedTrait *) { ++notifications; });
	{
		auto editor = volume.edit();
		EXPECT_THROW(editor.set({2, 0, 0}, voxel::Voxel::Cell(7)), spk::Exception);
	}

	EXPECT_TRUE(volume.at({0, 0, 0}).empty());
	EXPECT_TRUE(volume.at({1, 0, 0}).empty());
	EXPECT_EQ(volume.version(), spk::VersionedTrait::Version{0});
	EXPECT_EQ(notifications, 0);
}

TEST(VoxelVolumeEditorTest, ExplicitCommitClosesEditorAndNotifiesOnce)
{
	voxel::VoxelVolume volume({1, 1, 1}, 1.0f);
	int notifications = 0;
	auto contract = volume.subscribeToVersionEdition([&](spk::VersionedTrait *) { ++notifications; });
	auto editor = volume.edit();

	EXPECT_TRUE(editor.set({0, 0, 0}, voxel::Voxel::Cell(7)));
	editor.commit();
	editor.commit();
	EXPECT_THROW(editor.set({0, 0, 0}, voxel::Voxel::Cell(9)), spk::Exception);
	EXPECT_EQ(volume.version(), spk::VersionedTrait::Version{1});
	EXPECT_EQ(notifications, 1);
}
