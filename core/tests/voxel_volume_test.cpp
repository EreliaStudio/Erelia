#include "erelia/core/voxel/volume.hpp"
#include "erelia/core/voxel/volume_builder.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <cstdint>
#include <limits>
#include <span>
#include <type_traits>
#include <utility>

static_assert(std::is_same_v<Voxel::Volume::LocalCoordinate, spk::Vector3Int>);
static_assert(std::is_same_v<Voxel::Volume::UnitSize, float>);
static_assert(std::is_copy_constructible_v<Voxel::Volume>);
static_assert(std::is_copy_assignable_v<Voxel::Volume>);
static_assert(std::is_move_constructible_v<Voxel::Volume>);
static_assert(std::is_move_assignable_v<Voxel::Volume>);
static_assert(!std::is_copy_constructible_v<Voxel::Volume::Builder>);
static_assert(std::is_move_constructible_v<Voxel::Volume::Builder>);
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

	Voxel::Volume makeVolume(
		const spk::Vector3UInt &dimensions,
		Voxel::Volume::UnitSize unitSize)
	{
		Voxel::Volume::Builder builder(dimensions, unitSize);
		return std::move(builder).build();
	}

	Voxel::Volume makeVolumeWithCell(
		const spk::Vector3UInt &dimensions,
		Voxel::Volume::UnitSize unitSize,
		const Voxel::Volume::LocalCoordinate &coordinate,
		Voxel::Cell::PackedType packed)
	{
		Voxel::Volume::Builder builder(dimensions, unitSize);
		EXPECT_TRUE(builder.set(coordinate, Voxel::Cell(packed)));
		return std::move(builder).build();
	}
}

TEST(VoxelVolume, DefaultConstructionIsEmpty)
{
	const Voxel::Volume volume;

	expectDefaultVolume(volume);
	EXPECT_THROW((void)volume.at({0, 0, 0}), spk::Exception);
}

TEST(VoxelVolumeBuilder, ExplicitConstructionOwnsDefaultEmptyCells)
{
	Voxel::Volume::Builder builder({2, 3, 4}, 0.25f);
	const auto volume = std::move(builder).build();

	EXPECT_EQ(volume.dimensions(), (spk::Vector3UInt{2, 3, 4}));
	EXPECT_EQ(volume.unitSize(), 0.25f);
	ASSERT_EQ(volume.cells().size(), 24u);

	for (const auto &cell : volume.cells())
	{
		EXPECT_EQ(cell.packed(), Voxel::Cell::Empty.packed());
	}
}

TEST(VoxelVolumeBuilder, UsesYThenXThenZStorageOrder)
{
	Voxel::Volume::Builder builder({2, 3, 4}, 0.25f);

	EXPECT_TRUE(builder.set({0, 0, 0}, Voxel::Cell(11u)));
	EXPECT_TRUE(builder.set({0, 1, 0}, Voxel::Cell(12u)));
	EXPECT_TRUE(builder.set({1, 0, 0}, Voxel::Cell(13u)));
	EXPECT_TRUE(builder.set({0, 0, 1}, Voxel::Cell(14u)));
	EXPECT_TRUE(builder.set({1, 2, 3}, Voxel::Cell(15u)));

	const auto volume = std::move(builder).build();
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

TEST(VoxelVolumeBuilder, SetReportsOnlyEffectiveChanges)
{
	Voxel::Volume::Builder builder({1, 1, 1}, 1.0f);

	EXPECT_FALSE(builder.set({0, 0, 0}, Voxel::Cell::Empty));
	EXPECT_TRUE(builder.set({0, 0, 0}, Voxel::Cell(7u)));
	EXPECT_FALSE(builder.set({0, 0, 0}, Voxel::Cell(7u)));

	const auto volume = std::move(builder).build();
	EXPECT_EQ(volume.at({0, 0, 0}).packed(), 7u);
}

TEST(VoxelVolume, BracketAccessMatchesAtAndIsChecked)
{
	const auto volume = makeVolumeWithCell({2, 1, 1}, 1.0f, {1, 0, 0}, 42u);
	const Voxel::Volume::LocalCoordinate coordinate{1, 0, 0};

	EXPECT_EQ(volume[coordinate].packed(), volume.at(coordinate).packed());
	EXPECT_THROW(
		((void)volume[Voxel::Volume::LocalCoordinate{2, 0, 0}]),
		spk::Exception);
}

TEST(VoxelVolume, SupportsSingleCellBoundary)
{
	const auto volume = makeVolume({1, 1, 1}, 1.0f);

	EXPECT_TRUE(volume.contains({0, 0, 0}));
	EXPECT_FALSE(volume.contains({1, 0, 0}));
	EXPECT_FALSE(volume.contains({0, 1, 0}));
	EXPECT_FALSE(volume.contains({0, 0, 1}));
	EXPECT_FALSE(volume.contains({-1, 0, 0}));
	EXPECT_EQ(volume.at({0, 0, 0}).packed(), 0u);
}

TEST(VoxelVolumeBuilder, RejectsInvalidExplicitDimensions)
{
	EXPECT_THROW((Voxel::Volume::Builder({0, 1, 1}, 1.0f)), spk::Exception);
	EXPECT_THROW((Voxel::Volume::Builder({1, 0, 1}, 1.0f)), spk::Exception);
	EXPECT_THROW((Voxel::Volume::Builder({1, 1, 0}, 1.0f)), spk::Exception);

	constexpr auto maximum = std::numeric_limits<std::uint32_t>::max();
	EXPECT_THROW(
		(Voxel::Volume::Builder({maximum, maximum, maximum}, 1.0f)),
		spk::Exception);
}

TEST(VoxelVolumeBuilder, RejectsInvalidUnitSize)
{
	EXPECT_THROW((Voxel::Volume::Builder({1, 1, 1}, 0.0f)), spk::Exception);
	EXPECT_THROW((Voxel::Volume::Builder({1, 1, 1}, -1.0f)), spk::Exception);
	EXPECT_THROW(
		(Voxel::Volume::Builder(
			{1, 1, 1},
			std::numeric_limits<float>::quiet_NaN())),
		spk::Exception);
	EXPECT_THROW(
		(Voxel::Volume::Builder(
			{1, 1, 1},
			std::numeric_limits<float>::infinity())),
		spk::Exception);
	EXPECT_THROW(
		(Voxel::Volume::Builder(
			{1, 1, 1},
			-std::numeric_limits<float>::infinity())),
		spk::Exception);
}

TEST(VoxelVolume, RejectsInvalidCheckedCoordinates)
{
	const auto volume = makeVolume({2, 3, 4}, 1.0f);

	EXPECT_THROW((void)volume.at({-1, 0, 0}), spk::Exception);
	EXPECT_THROW((void)volume.at({0, -1, 0}), spk::Exception);
	EXPECT_THROW((void)volume.at({0, 0, -1}), spk::Exception);
	EXPECT_THROW((void)volume.at({2, 0, 0}), spk::Exception);
	EXPECT_THROW((void)volume.at({0, 3, 0}), spk::Exception);
	EXPECT_THROW((void)volume.at({0, 0, 4}), spk::Exception);
}

TEST(VoxelVolumeBuilder, RejectsInvalidCoordinatesWithoutMutatingOtherCells)
{
	Voxel::Volume::Builder builder({2, 1, 1}, 1.0f);

	EXPECT_THROW((void)builder.set({2, 0, 0}, Voxel::Cell(7u)), spk::Exception);
	EXPECT_TRUE(builder.set({1, 0, 0}, Voxel::Cell(9u)));

	const auto volume = std::move(builder).build();
	EXPECT_EQ(volume.at({0, 0, 0}).packed(), 0u);
	EXPECT_EQ(volume.at({1, 0, 0}).packed(), 9u);
}

TEST(VoxelVolume, CopiesShareImmutableContent)
{
	const auto source = makeVolumeWithCell({2, 1, 1}, 0.5f, {1, 0, 0}, 17u);
	const Voxel::Volume copy(source);

	EXPECT_EQ(copy.dimensions(), source.dimensions());
	EXPECT_EQ(copy.unitSize(), source.unitSize());
	EXPECT_EQ(copy.cells()[1].packed(), 17u);
	EXPECT_EQ(copy.cells().data(), source.cells().data());
}

TEST(VoxelVolume, CopyAssignmentSharesImmutableContent)
{
	const auto source = makeVolumeWithCell({2, 1, 1}, 0.5f, {1, 0, 0}, 17u);
	Voxel::Volume destination = makeVolume({1, 1, 1}, 1.0f);

	destination = source;

	EXPECT_EQ(destination.dimensions(), source.dimensions());
	EXPECT_EQ(destination.unitSize(), source.unitSize());
	EXPECT_EQ(destination.cells().data(), source.cells().data());
	EXPECT_EQ(destination.cells()[1].packed(), 17u);
}

TEST(VoxelVolume, MoveTransfersContentAndLeavesSourceEmpty)
{
	auto source = makeVolumeWithCell({2, 1, 1}, 0.5f, {1, 0, 0}, 17u);
	const auto *sourceData = source.cells().data();

	Voxel::Volume moved(std::move(source));

	EXPECT_EQ(moved.dimensions(), (spk::Vector3UInt{2, 1, 1}));
	EXPECT_EQ(moved.unitSize(), 0.5f);
	EXPECT_EQ(moved.cells().data(), sourceData);
	EXPECT_EQ(moved.cells()[1].packed(), 17u);
	expectDefaultVolume(source);
}

TEST(VoxelVolumeBuilder, MovingUniqueVolumeIntoBuilderReusesItsBuffer)
{
	auto volume = makeVolumeWithCell({2, 1, 1}, 0.5f, {1, 0, 0}, 17u);
	const auto *originalData = volume.cells().data();

	Voxel::Volume::Builder builder(std::move(volume));
	expectDefaultVolume(volume);

	EXPECT_TRUE(builder.set({0, 0, 0}, Voxel::Cell(23u)));
	const auto rebuilt = std::move(builder).build();

	EXPECT_EQ(rebuilt.cells().data(), originalData);
	EXPECT_EQ(rebuilt.at({0, 0, 0}).packed(), 23u);
	EXPECT_EQ(rebuilt.at({1, 0, 0}).packed(), 17u);
}

TEST(VoxelVolumeBuilder, MovingSharedVolumeIntoBuilderCopiesItsBuffer)
{
	auto volume = makeVolumeWithCell({2, 1, 1}, 0.5f, {1, 0, 0}, 17u);
	const Voxel::Volume sharedCopy(volume);
	const auto *sharedData = sharedCopy.cells().data();

	Voxel::Volume::Builder builder(std::move(volume));
	expectDefaultVolume(volume);

	EXPECT_TRUE(builder.set({0, 0, 0}, Voxel::Cell(23u)));
	const auto rebuilt = std::move(builder).build();

	EXPECT_NE(rebuilt.cells().data(), sharedData);
	EXPECT_EQ(rebuilt.at({0, 0, 0}).packed(), 23u);
	EXPECT_EQ(rebuilt.at({1, 0, 0}).packed(), 17u);
	EXPECT_EQ(sharedCopy.at({0, 0, 0}).packed(), 0u);
	EXPECT_EQ(sharedCopy.at({1, 0, 0}).packed(), 17u);
}

TEST(VoxelVolumeBuilder, ChunkSizedBuffersUseTheirDedicatedPool)
{
	const Voxel::Cell *chunkData = nullptr;

	{
		auto chunkVolume = makeVolume({16, 16, 16}, 1.0f);
		chunkData = chunkVolume.cells().data();
		ASSERT_NE(chunkData, nullptr);
	}

	const auto nonChunkVolume = makeVolume({8, 8, 64}, 1.0f);
	EXPECT_NE(nonChunkVolume.cells().data(), chunkData);

	const auto reusedChunkVolume = makeVolume({16, 16, 16}, 1.0f);
	EXPECT_EQ(reusedChunkVolume.cells().data(), chunkData);
}

TEST(VoxelVolumeBuilder, GeneralPoolUsesSmallestAvailableHigherSizeClass)
{
	const Voxel::Cell *largerBufferData = nullptr;

	{
		auto largerVolume = makeVolume({10, 10, 50}, 1.0f);
		largerBufferData = largerVolume.cells().data();
		ASSERT_NE(largerBufferData, nullptr);
	}

	const auto smallerVolume = makeVolume({9, 10, 50}, 1.0f);

	ASSERT_EQ(smallerVolume.cells().size(), 4500u);
	EXPECT_EQ(smallerVolume.cells().data(), largerBufferData);
}
