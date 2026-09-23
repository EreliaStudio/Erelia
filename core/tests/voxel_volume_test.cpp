#include "erelia/core/voxel/volume.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <cstdint>
#include <limits>
#include <span>
#include <type_traits>
#include <utility>

static_assert(std::is_same_v<Voxel::Volume::LocalCoordinate, spk::Vector3Int>);
static_assert(std::is_same_v<Voxel::Volume::UnitSize, float>);
static_assert(std::is_base_of_v<spk::VersionedTrait, Voxel::Volume>);
static_assert(std::is_copy_constructible_v<Voxel::Volume>);
static_assert(std::is_copy_assignable_v<Voxel::Volume>);
static_assert(std::is_move_constructible_v<Voxel::Volume>);
static_assert(std::is_move_assignable_v<Voxel::Volume>);
static_assert(
	std::is_same_v<decltype(std::declval<const Voxel::Volume &>().cells()), std::span<const Voxel::Cell>>);

namespace
{
	void expectDefaultVolume(const Voxel::Volume &volume)
	{
		EXPECT_EQ(volume.dimensions(), (spk::Vector3UInt{0, 0, 0}));
		EXPECT_EQ(volume.unitSize(), 0.0f);
		EXPECT_TRUE(volume.cells().empty());
		EXPECT_FALSE(volume.contains({0, 0, 0}));
		EXPECT_FALSE(volume.contains({-1, 0, 0}));
	}

	void setCell(
		Voxel::Volume &volume,
		const Voxel::Volume::LocalCoordinate &coordinate,
		Voxel::Cell::PackedType packed)
	{
		auto editor = volume.edit();
		EXPECT_TRUE(editor.set(coordinate, Voxel::Cell(packed)));
		editor.commit();
	}
}

TEST(VoxelVolume, DefaultConstructionIsTheOnlyEmptyState)
{
	Voxel::Volume volume;

	expectDefaultVolume(volume);
	EXPECT_EQ(volume.version(), spk::VersionedTrait::Version{0});
	EXPECT_THROW(volume.at({0, 0, 0}), spk::Exception);

	auto editor = volume.edit();
	EXPECT_THROW(editor.set({0, 0, 0}, Voxel::Cell(1u)), spk::Exception);
	EXPECT_EQ(volume.version(), spk::VersionedTrait::Version{0});
}

TEST(VoxelVolume, ExplicitConstructionOwnsDefaultEmptyCells)
{
	const Voxel::Volume volume({2, 3, 4}, 0.25f);

	EXPECT_EQ(volume.dimensions(), (spk::Vector3UInt{2, 3, 4}));
	EXPECT_EQ(volume.unitSize(), 0.25f);
	ASSERT_EQ(volume.cells().size(), 24u);
	for (const auto &cell : volume.cells())
	{
		EXPECT_EQ(cell.packed(), Voxel::Cell::Empty.packed());
	}
	EXPECT_EQ(volume.version(), spk::VersionedTrait::Version{0});
}

TEST(VoxelVolume, UsesYThenXThenZStorageOrder)
{
	Voxel::Volume volume({2, 3, 4}, 0.25f);
	auto editor = volume.edit();

	EXPECT_TRUE(editor.set({0, 0, 0}, Voxel::Cell(11u)));
	EXPECT_TRUE(editor.set({0, 1, 0}, Voxel::Cell(12u)));
	EXPECT_TRUE(editor.set({1, 0, 0}, Voxel::Cell(13u)));
	EXPECT_TRUE(editor.set({0, 0, 1}, Voxel::Cell(14u)));
	EXPECT_TRUE(editor.set({1, 2, 3}, Voxel::Cell(15u)));
	editor.commit();

	const auto cells = volume.cells();
	ASSERT_EQ(cells.size(), 24u);
	EXPECT_EQ(cells[0].packed(), 11u);
	EXPECT_EQ(cells[1].packed(), 12u);
	EXPECT_EQ(cells[3].packed(), 13u);
	EXPECT_EQ(cells[6].packed(), 14u);
	EXPECT_EQ(cells[23].packed(), 15u);

	EXPECT_EQ(volume.at({0, 0, 0}).packed(), 11u);
	EXPECT_EQ(volume.at({0, 1, 0}).packed(), 12u);
	EXPECT_EQ(volume.at({1, 0, 0}).packed(), 13u);
	EXPECT_EQ(volume.at({0, 0, 1}).packed(), 14u);
	EXPECT_EQ(volume.at({1, 2, 3}).packed(), 15u);
}

TEST(VoxelVolume, BracketAccessMatchesAtAndIsChecked)
{
	Voxel::Volume volume({2, 1, 1}, 1.0f);
	setCell(volume, {1, 0, 0}, 42u);
	const auto &readOnlyVolume = volume;
	const Voxel::Volume::LocalCoordinate coordinate{1, 0, 0};

	EXPECT_EQ(readOnlyVolume[coordinate].packed(), readOnlyVolume.at(coordinate).packed());
	EXPECT_THROW(
		readOnlyVolume[Voxel::Volume::LocalCoordinate{2, 0, 0}],
		spk::Exception);
}

TEST(VoxelVolume, SupportsSingleCellBoundary)
{
	Voxel::Volume volume({1, 1, 1}, 1.0f);

	EXPECT_TRUE(volume.contains({0, 0, 0}));
	EXPECT_FALSE(volume.contains({1, 0, 0}));
	EXPECT_FALSE(volume.contains({0, 1, 0}));
	EXPECT_FALSE(volume.contains({0, 0, 1}));
	EXPECT_FALSE(volume.contains({-1, 0, 0}));
	EXPECT_EQ(volume.at({0, 0, 0}).packed(), 0u);
}

TEST(VoxelVolume, RejectsInvalidExplicitDimensions)
{
	EXPECT_THROW((Voxel::Volume({0, 1, 1}, 1.0f)), spk::Exception);
	EXPECT_THROW((Voxel::Volume({1, 0, 1}, 1.0f)), spk::Exception);
	EXPECT_THROW((Voxel::Volume({1, 1, 0}, 1.0f)), spk::Exception);

	constexpr auto maximum = std::numeric_limits<std::uint32_t>::max();
	EXPECT_THROW((Voxel::Volume({maximum, maximum, maximum}, 1.0f)), spk::Exception);
}

TEST(VoxelVolume, RejectsInvalidUnitSize)
{
	EXPECT_THROW((Voxel::Volume({1, 1, 1}, 0.0f)), spk::Exception);
	EXPECT_THROW((Voxel::Volume({1, 1, 1}, -1.0f)), spk::Exception);
	EXPECT_THROW(
		(Voxel::Volume({1, 1, 1}, std::numeric_limits<float>::quiet_NaN())),
		spk::Exception);
	EXPECT_THROW(
		(Voxel::Volume({1, 1, 1}, std::numeric_limits<float>::infinity())),
		spk::Exception);
	EXPECT_THROW(
		(Voxel::Volume({1, 1, 1}, -std::numeric_limits<float>::infinity())),
		spk::Exception);
}

TEST(VoxelVolume, RejectsInvalidCheckedCoordinates)
{
	const Voxel::Volume volume({2, 3, 4}, 1.0f);

	EXPECT_THROW(volume.at({-1, 0, 0}), spk::Exception);
	EXPECT_THROW(volume.at({0, -1, 0}), spk::Exception);
	EXPECT_THROW(volume.at({0, 0, -1}), spk::Exception);
	EXPECT_THROW(volume.at({2, 0, 0}), spk::Exception);
	EXPECT_THROW(volume.at({0, 3, 0}), spk::Exception);
	EXPECT_THROW(volume.at({0, 0, 4}), spk::Exception);
}

TEST(VoxelVolumeEditor, BatchesEffectiveWritesIntoOneVersion)
{
	Voxel::Volume volume({2, 1, 1}, 1.0f);
	int notifications = 0;
	auto contract = volume.subscribeToVersionEdition([&](spk::VersionedTrait *versioned) {
		++notifications;
		EXPECT_EQ(versioned, &volume);
	});

	auto editor = volume.edit();
	EXPECT_TRUE(editor.set({0, 0, 0}, Voxel::Cell(7u)));
	EXPECT_FALSE(editor.set({0, 0, 0}, Voxel::Cell(7u)));
	EXPECT_TRUE(editor.set({1, 0, 0}, Voxel::Cell(9u)));
	editor.commit();
	editor.commit();

	EXPECT_EQ(volume.version(), spk::VersionedTrait::Version{1});
	EXPECT_EQ(notifications, 1);
	EXPECT_THROW(editor.set({0, 0, 0}, Voxel::Cell(3u)), spk::Exception);
}

TEST(VoxelVolumeEditor, DestructionCommitsChangedBatch)
{
	Voxel::Volume volume({1, 1, 1}, 1.0f);
	int notifications = 0;
	auto contract = volume.subscribeToVersionEdition([&](spk::VersionedTrait *) {
		++notifications;
	});

	{
		auto editor = volume.edit();
		EXPECT_TRUE(editor.set({0, 0, 0}, Voxel::Cell(7u)));
	}

	EXPECT_EQ(volume.version(), spk::VersionedTrait::Version{1});
	EXPECT_EQ(notifications, 1);
	EXPECT_EQ(volume.at({0, 0, 0}).packed(), 7u);
}

TEST(VoxelVolumeEditor, NoOpBatchDoesNotInvalidate)
{
	Voxel::Volume volume({1, 1, 1}, 1.0f);
	int notifications = 0;
	auto contract = volume.subscribeToVersionEdition([&](spk::VersionedTrait *) {
		++notifications;
	});

	auto editor = volume.edit();
	EXPECT_FALSE(editor.set({0, 0, 0}, Voxel::Cell::Empty));
	editor.commit();

	EXPECT_EQ(volume.version(), spk::VersionedTrait::Version{0});
	EXPECT_EQ(notifications, 0);
}

TEST(VoxelVolumeEditor, InvalidWriteDoesNotAliasAndEditorRemainsUsable)
{
	Voxel::Volume volume({2, 1, 1}, 1.0f);
	int notifications = 0;
	auto contract = volume.subscribeToVersionEdition([&](spk::VersionedTrait *) {
		++notifications;
	});

	auto editor = volume.edit();
	EXPECT_THROW(editor.set({2, 0, 0}, Voxel::Cell(7u)), spk::Exception);
	EXPECT_EQ(volume.at({0, 0, 0}).packed(), 0u);
	EXPECT_EQ(volume.at({1, 0, 0}).packed(), 0u);
	EXPECT_EQ(volume.version(), spk::VersionedTrait::Version{0});
	EXPECT_EQ(notifications, 0);

	EXPECT_TRUE(editor.set({1, 0, 0}, Voxel::Cell(9u)));
	editor.commit();

	EXPECT_EQ(volume.at({0, 0, 0}).packed(), 0u);
	EXPECT_EQ(volume.at({1, 0, 0}).packed(), 9u);
	EXPECT_EQ(volume.version(), spk::VersionedTrait::Version{1});
	EXPECT_EQ(notifications, 1);
}

TEST(VoxelVolume, CellSpanSurvivesOrdinaryEdits)
{
	Voxel::Volume volume({2, 1, 1}, 1.0f);
	const auto cells = volume.cells();
	const auto *data = cells.data();

	setCell(volume, {1, 0, 0}, 42u);

	EXPECT_EQ(cells.data(), data);
	EXPECT_EQ(cells[1].packed(), 42u);
}

TEST(VoxelVolume, CopyConstructionCreatesIndependentFreshVersion)
{
	Voxel::Volume source({2, 1, 1}, 0.5f);
	setCell(source, {1, 0, 0}, 17u);
	int sourceNotifications = 0;
	auto sourceContract = source.subscribeToVersionEdition([&](spk::VersionedTrait *) {
		++sourceNotifications;
	});

	Voxel::Volume copy(source);

	EXPECT_EQ(copy.dimensions(), source.dimensions());
	EXPECT_EQ(copy.unitSize(), source.unitSize());
	EXPECT_EQ(copy.cells()[1].packed(), 17u);
	EXPECT_NE(copy.cells().data(), source.cells().data());
	EXPECT_EQ(copy.version(), spk::VersionedTrait::Version{0});

	setCell(copy, {0, 0, 0}, 33u);
	EXPECT_EQ(source.at({0, 0, 0}).packed(), 0u);
	EXPECT_EQ(sourceNotifications, 0);
}

TEST(VoxelVolume, CopyAssignmentPreservesDestinationSubscriptions)
{
	Voxel::Volume source({2, 1, 1}, 0.5f);
	setCell(source, {1, 0, 0}, 17u);
	const auto sourceVersion = source.version();

	Voxel::Volume destination({1, 1, 1}, 1.0f);
	int notifications = 0;
	auto contract = destination.subscribeToVersionEdition([&](spk::VersionedTrait *versioned) {
		++notifications;
		EXPECT_EQ(versioned, &destination);
	});

	destination = source;

	EXPECT_EQ(destination.dimensions(), source.dimensions());
	EXPECT_EQ(destination.unitSize(), source.unitSize());
	EXPECT_EQ(destination.cells()[1].packed(), 17u);
	EXPECT_NE(destination.cells().data(), source.cells().data());
	EXPECT_EQ(destination.version(), spk::VersionedTrait::Version{1});
	EXPECT_EQ(notifications, 1);
	EXPECT_EQ(source.version(), sourceVersion);
}

TEST(VoxelVolume, MoveConstructionTransfersStorageAndInvalidatesSource)
{
	Voxel::Volume source({2, 1, 1}, 0.5f);
	setCell(source, {1, 0, 0}, 17u);
	const auto sourceVersion = source.version();
	const auto *sourceData = source.cells().data();
	int sourceNotifications = 0;
	auto sourceContract = source.subscribeToVersionEdition([&](spk::VersionedTrait *versioned) {
		++sourceNotifications;
		EXPECT_EQ(versioned, &source);
	});

	Voxel::Volume moved(std::move(source));

	EXPECT_EQ(moved.dimensions(), (spk::Vector3UInt{2, 1, 1}));
	EXPECT_EQ(moved.unitSize(), 0.5f);
	EXPECT_EQ(moved.cells()[1].packed(), 17u);
	EXPECT_EQ(moved.cells().data(), sourceData);
	EXPECT_EQ(moved.version(), spk::VersionedTrait::Version{0});

	expectDefaultVolume(source);
	EXPECT_EQ(source.version(), sourceVersion + 1);
	EXPECT_EQ(sourceNotifications, 1);
}

TEST(VoxelVolume, MoveAssignmentInvalidatesBothObjects)
{
	Voxel::Volume source({2, 1, 1}, 0.5f);
	setCell(source, {1, 0, 0}, 17u);
	const auto sourceVersion = source.version();
	int sourceNotifications = 0;
	auto sourceContract = source.subscribeToVersionEdition([&](spk::VersionedTrait *versioned) {
		++sourceNotifications;
		EXPECT_EQ(versioned, &source);
	});

	Voxel::Volume destination({1, 1, 1}, 1.0f);
	const auto destinationVersion = destination.version();
	int destinationNotifications = 0;
	auto destinationContract = destination.subscribeToVersionEdition([&](spk::VersionedTrait *versioned) {
		++destinationNotifications;
		EXPECT_EQ(versioned, &destination);
	});

	destination = std::move(source);

	EXPECT_EQ(destination.dimensions(), (spk::Vector3UInt{2, 1, 1}));
	EXPECT_EQ(destination.unitSize(), 0.5f);
	EXPECT_EQ(destination.cells()[1].packed(), 17u);
	EXPECT_EQ(destination.version(), destinationVersion + 1);
	EXPECT_EQ(destinationNotifications, 1);

	expectDefaultVolume(source);
	EXPECT_EQ(source.version(), sourceVersion + 1);
	EXPECT_EQ(sourceNotifications, 1);
}

TEST(VoxelVolume, SelfAssignmentIsANoOp)
{
	Voxel::Volume volume({1, 1, 1}, 1.0f);
	setCell(volume, {0, 0, 0}, 7u);
	const auto initialVersion = volume.version();
	const auto *initialData = volume.cells().data();
	int notifications = 0;
	auto contract = volume.subscribeToVersionEdition([&](spk::VersionedTrait *) {
		++notifications;
	});

	volume = volume;
	EXPECT_EQ(volume.version(), initialVersion);
	EXPECT_EQ(volume.cells().data(), initialData);
	EXPECT_EQ(notifications, 0);

	volume = std::move(volume);
	EXPECT_EQ(volume.version(), initialVersion);
	EXPECT_EQ(volume.cells().data(), initialData);
	EXPECT_EQ(volume.at({0, 0, 0}).packed(), 7u);
	EXPECT_EQ(notifications, 0);
}
