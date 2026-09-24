#include "erelia/core/voxel/cell.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <array>
#include <cstdint>
#include <type_traits>

static_assert(std::is_same_v<Voxel::Definition::ID, std::uint32_t>);
static_assert(std::is_same_v<Voxel::Cell::PackedType, std::uint32_t>);
static_assert(sizeof(Voxel::Cell) == sizeof(Voxel::Cell::PackedType));
static_assert(std::is_trivially_copyable_v<Voxel::Cell>);
static_assert(static_cast<std::uint8_t>(Voxel::Cell::Orientation::PositiveX) == 0);
static_assert(static_cast<std::uint8_t>(Voxel::Cell::Orientation::NegativeX) == 1);
static_assert(static_cast<std::uint8_t>(Voxel::Cell::Orientation::PositiveZ) == 2);
static_assert(static_cast<std::uint8_t>(Voxel::Cell::Orientation::NegativeZ) == 3);
static_assert(static_cast<std::uint8_t>(Voxel::Cell::FlipOrientation::PositiveY) == 0);
static_assert(static_cast<std::uint8_t>(Voxel::Cell::FlipOrientation::NegativeY) == 1);

namespace
{
	struct PackingFixture
	{
		Voxel::Definition::ID definitionId;
		Voxel::Cell::Orientation orientation;
		Voxel::Cell::FlipOrientation flipOrientation;
		Voxel::Cell::PackedType packed;
	};

	constexpr std::array<PackingFixture, 8> packingFixtures = {{
		{1u, Voxel::Cell::Orientation::PositiveX, Voxel::Cell::FlipOrientation::PositiveY, 0x00000001u},
		{1u, Voxel::Cell::Orientation::NegativeX, Voxel::Cell::FlipOrientation::PositiveY, 0x20000001u},
		{1u, Voxel::Cell::Orientation::PositiveZ, Voxel::Cell::FlipOrientation::PositiveY, 0x40000001u},
		{1u, Voxel::Cell::Orientation::NegativeZ, Voxel::Cell::FlipOrientation::PositiveY, 0x60000001u},
		{1u, Voxel::Cell::Orientation::PositiveX, Voxel::Cell::FlipOrientation::NegativeY, 0x80000001u},
		{1u, Voxel::Cell::Orientation::NegativeX, Voxel::Cell::FlipOrientation::NegativeY, 0xA0000001u},
		{1u, Voxel::Cell::Orientation::PositiveZ, Voxel::Cell::FlipOrientation::NegativeY, 0xC0000001u},
		{1u, Voxel::Cell::Orientation::NegativeZ, Voxel::Cell::FlipOrientation::NegativeY, 0xE0000001u},
	}};
}

TEST(VoxelCell, DefaultAndExplicitEmptyArePackedZero)
{
	const Voxel::Cell defaultCell;

	EXPECT_EQ(defaultCell.packed(), 0x00000000u);
	EXPECT_EQ(defaultCell.definitionId(), 0u);
	EXPECT_EQ(defaultCell.orientation(), Voxel::Cell::Orientation::PositiveX);
	EXPECT_EQ(defaultCell.flipOrientation(), Voxel::Cell::FlipOrientation::PositiveY);

	EXPECT_EQ(Voxel::Cell::Empty.packed(), 0x00000000u);
	EXPECT_EQ(Voxel::Cell::Empty.definitionId(), 0u);
	EXPECT_EQ(Voxel::Cell::Empty.orientation(), Voxel::Cell::Orientation::PositiveX);
	EXPECT_EQ(Voxel::Cell::Empty.flipOrientation(), Voxel::Cell::FlipOrientation::PositiveY);
}

TEST(VoxelCell, PacksEveryOrientationAndFlipCombinationExactly)
{
	for (const auto &fixture : packingFixtures)
	{
		const Voxel::Cell cell(fixture.definitionId, fixture.orientation, fixture.flipOrientation);

		EXPECT_EQ(cell.definitionId(), fixture.definitionId);
		EXPECT_EQ(cell.orientation(), fixture.orientation);
		EXPECT_EQ(cell.flipOrientation(), fixture.flipOrientation);
		EXPECT_EQ(cell.packed(), fixture.packed);
	}
}

TEST(VoxelCell, PreservesOrientedEmptyPackedValue)
{
	const Voxel::Cell cell(0x60000000u);

	EXPECT_EQ(cell.definitionId(), 0u);
	EXPECT_EQ(cell.orientation(), Voxel::Cell::Orientation::NegativeZ);
	EXPECT_EQ(cell.flipOrientation(), Voxel::Cell::FlipOrientation::PositiveY);
	EXPECT_EQ(cell.packed(), 0x60000000u);
}

TEST(VoxelCell, AcceptsAndPreservesAnyRawPackedBits)
{
	constexpr std::array<Voxel::Cell::PackedType, 4> packedValues = {
		0x00000000u,
		0x60000000u,
		0xA1234567u,
		0xFFFFFFFFu,
	};

	for (const auto packed : packedValues)
	{
		const Voxel::Cell cell(packed);
		EXPECT_EQ(cell.packed(), packed);
	}
}

TEST(VoxelCell, DecodesMaximumPackedValue)
{
	const Voxel::Cell cell(0xFFFFFFFFu);

	EXPECT_EQ(cell.definitionId(), 0x1FFFFFFFu);
	EXPECT_EQ(cell.orientation(), Voxel::Cell::Orientation::NegativeZ);
	EXPECT_EQ(cell.flipOrientation(), Voxel::Cell::FlipOrientation::NegativeY);
	EXPECT_EQ(cell.packed(), 0xFFFFFFFFu);
}

TEST(VoxelCell, PacksMaximumLogicalDefinitionId)
{
	const Voxel::Cell cell(
		0x1FFFFFFFu,
		Voxel::Cell::Orientation::NegativeZ,
		Voxel::Cell::FlipOrientation::NegativeY);

	EXPECT_EQ(cell.packed(), 0xFFFFFFFFu);
}

TEST(VoxelCell, RejectsDefinitionIdOutsidePackedCapacity)
{
	EXPECT_THROW(
		(Voxel::Cell(
			0x20000000u,
			Voxel::Cell::Orientation::PositiveX,
			Voxel::Cell::FlipOrientation::PositiveY)),
		spk::Exception);
}

TEST(VoxelCell, RejectsInvalidOrientation)
{
	EXPECT_THROW(
		(Voxel::Cell(
			1u,
			static_cast<Voxel::Cell::Orientation>(4),
			Voxel::Cell::FlipOrientation::PositiveY)),
		spk::Exception);
}

TEST(VoxelCell, RejectsInvalidFlipOrientation)
{
	EXPECT_THROW(
		(Voxel::Cell(
			1u,
			Voxel::Cell::Orientation::PositiveX,
			static_cast<Voxel::Cell::FlipOrientation>(2))),
		spk::Exception);
}

TEST(VoxelCell, LogicalPackingIsDeterministic)
{
	const Voxel::Cell first(
		0x01234567u,
		Voxel::Cell::Orientation::PositiveZ,
		Voxel::Cell::FlipOrientation::NegativeY);
	const Voxel::Cell second(
		0x01234567u,
		Voxel::Cell::Orientation::PositiveZ,
		Voxel::Cell::FlipOrientation::NegativeY);

	EXPECT_EQ(first.packed(), second.packed());
	EXPECT_EQ(first.definitionId(), second.definitionId());
	EXPECT_EQ(first.orientation(), second.orientation());
	EXPECT_EQ(first.flipOrientation(), second.flipOrientation());
}
