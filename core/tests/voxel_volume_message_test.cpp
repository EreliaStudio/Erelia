#include "erelia/core/voxel/volume.hpp"
#include "erelia/core/voxel/volume_builder.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>
#include <network/message.hpp>

#include <algorithm>
#include <array>
#include <cstddef>
#include <cstdint>
#include <limits>
#include <utility>
#include <vector>

namespace
{
	Voxel::Volume makeAsymmetricVolume()
	{
		Voxel::Volume::Builder builder({3, 2, 2}, 0.25f);

		Voxel::Cell::PackedType packed = 1;
		for (std::int32_t z = 0; z < 2; ++z)
		{
			for (std::int32_t x = 0; x < 3; ++x)
			{
				for (std::int32_t y = 0; y < 2; ++y)
				{
					EXPECT_TRUE(builder.set({x, y, z}, Voxel::Cell(packed)));
					++packed;
				}
			}
		}

		return std::move(builder).build();
	}

	Voxel::Volume makeSentinelVolume()
	{
		Voxel::Volume::Builder builder({2, 1, 1}, 0.5f);
		EXPECT_TRUE(builder.set({0, 0, 0}, Voxel::Cell(0x12345678u)));
		EXPECT_TRUE(builder.set({1, 0, 0}, Voxel::Cell(0x87654321u)));
		return std::move(builder).build();
	}

	void expectVolumesEqual(
		const Voxel::Volume &actual,
		const Voxel::Volume &expected)
	{
		EXPECT_EQ(actual.dimensions(), expected.dimensions());
		EXPECT_EQ(actual.unitSize(), expected.unitSize());

		const auto actualCells = actual.cells();
		const auto expectedCells = expected.cells();

		ASSERT_EQ(actualCells.size(), expectedCells.size());
		for (std::size_t index = 0; index < actualCells.size(); ++index)
		{
			EXPECT_EQ(actualCells[index].packed(), expectedCells[index].packed());
		}
	}

	void expectDecodeFailurePreservesDestination(spk::Message &message)
	{
		Voxel::Volume destination = makeSentinelVolume();
		const Voxel::Volume expected(destination);

		EXPECT_THROW((message >> destination), spk::Exception);
		expectVolumesEqual(destination, expected);
	}

	spk::Message metadataMessage(
		const spk::Vector3UInt &dimensions,
		Voxel::Volume::UnitSize unitSize)
	{
		spk::Message message;
		message << dimensions;
		message << unitSize;
		return message;
	}
}

TEST(VoxelVolumeMessage, EmptyVolumeRoundTripsThroughDirectOperators)
{
	const Voxel::Volume source;
	spk::Message message(73u);

	message << source;

	EXPECT_EQ(message.type(), 73u);
	EXPECT_EQ(
		message.size(),
		sizeof(spk::Vector3UInt) + sizeof(Voxel::Volume::UnitSize));

	Voxel::Volume destination = makeSentinelVolume();
	message >> destination;

	EXPECT_EQ(message.type(), 73u);
	EXPECT_EQ(destination.dimensions(), (spk::Vector3UInt{0, 0, 0}));
	EXPECT_EQ(destination.unitSize(), 0.0f);
	EXPECT_TRUE(destination.cells().empty());
}

TEST(VoxelVolumeMessage, ConstructsVolumeDirectlyFromMessage)
{
	const auto source = makeAsymmetricVolume();
	spk::Message message;

	message << source;

	const Voxel::Volume destination(message);

	expectVolumesEqual(destination, source);
	EXPECT_EQ(message.readOffset(), message.size());
}

TEST(VoxelVolumeMessage, MessageConstructorUsesTheSameValidationContract)
{
	spk::Message message = metadataMessage({0, 1, 1}, 1.0f);

	EXPECT_THROW((void)Voxel::Volume(message), spk::Exception);
}

TEST(VoxelVolumeMessage, SerializesNativeMetadataThenContiguousYThenXThenZCells)
{
	const auto source = makeAsymmetricVolume();
	spk::Message message;

	message << source;

	EXPECT_EQ(
		message.size(),
		sizeof(spk::Vector3UInt) +
			sizeof(Voxel::Volume::UnitSize) +
			source.cells().size_bytes());

	spk::Vector3UInt dimensions{};
	Voxel::Volume::UnitSize unitSize = 0.0f;
	std::array<Voxel::Cell, 12> cells{};

	message >> dimensions;
	message >> unitSize;
	message.pull(cells.data(), sizeof(cells));

	EXPECT_EQ(dimensions, (spk::Vector3UInt{3, 2, 2}));
	EXPECT_EQ(unitSize, 0.25f);

	for (std::size_t index = 0; index < cells.size(); ++index)
	{
		EXPECT_EQ(cells[index].packed(), index + 1);
	}

	EXPECT_EQ(message.readOffset(), message.size());
}

TEST(VoxelVolumeMessage, LogicalCellPackingRoundTripsExactly)
{
	Voxel::Volume::Builder builder({3, 1, 1}, 1.5f);
	const Voxel::Cell first(
		17u,
		Voxel::Cell::Orientation::NegativeZ,
		Voxel::Cell::FlipOrientation::NegativeY);
	const Voxel::Cell second(
		42u,
		Voxel::Cell::Orientation::PositiveZ,
		Voxel::Cell::FlipOrientation::PositiveY);
	const Voxel::Cell third(
		(1u << 29u) - 1u,
		Voxel::Cell::Orientation::NegativeX,
		Voxel::Cell::FlipOrientation::NegativeY);

	ASSERT_TRUE(builder.set({0, 0, 0}, first));
	ASSERT_TRUE(builder.set({1, 0, 0}, second));
	ASSERT_TRUE(builder.set({2, 0, 0}, third));
	const auto source = std::move(builder).build();

	spk::Message message;
	message << source;

	Voxel::Volume destination;
	message >> destination;

	expectVolumesEqual(destination, source);
	EXPECT_EQ(destination.cells()[0].packed(), first.packed());
	EXPECT_EQ(destination.cells()[1].packed(), second.packed());
	EXPECT_EQ(destination.cells()[2].packed(), third.packed());
}

TEST(VoxelVolumeMessage, ChunkSizedVolumeRoundTripsAll4096Cells)
{
	Voxel::Volume::Builder builder({16, 16, 16}, 1.0f);

	for (std::int32_t z = 0; z < 16; ++z)
	{
		for (std::int32_t x = 0; x < 16; ++x)
		{
			for (std::int32_t y = 0; y < 16; ++y)
			{
				const auto packed = static_cast<Voxel::Cell::PackedType>(
					1 + y + 16 * (x + 16 * z));
				ASSERT_TRUE(builder.set({x, y, z}, Voxel::Cell(packed)));
			}
		}
	}

	const auto source = std::move(builder).build();
	ASSERT_EQ(source.cells().size(), 4096u);

	spk::Message message;
	message << source;

	Voxel::Volume destination;
	message >> destination;

	expectVolumesEqual(destination, source);
}

TEST(VoxelVolumeMessage, RepeatedSerializationIsByteStableOnCurrentAbi)
{
	const auto source = makeAsymmetricVolume();
	spk::Message first;
	spk::Message second;

	first << source;
	second << source;

	ASSERT_EQ(first.size(), second.size());
	for (std::size_t index = 0; index < first.size(); ++index)
	{
		EXPECT_EQ(first.data()[index], second.data()[index]);
	}
}

TEST(VoxelVolumeMessage, VolumeRemainsEmbeddableInsideLargerMessage)
{
	const std::uint32_t prefix = 0x11223344u;
	const std::uint32_t suffix = 0x55667788u;
	const auto source = makeAsymmetricVolume();

	spk::Message message;
	message << prefix;
	message << source;
	message << suffix;

	std::uint32_t decodedPrefix = 0;
	std::uint32_t decodedSuffix = 0;
	Voxel::Volume destination;

	message >> decodedPrefix;
	message >> destination;
	message >> decodedSuffix;

	EXPECT_EQ(decodedPrefix, prefix);
	expectVolumesEqual(destination, source);
	EXPECT_EQ(decodedSuffix, suffix);
	EXPECT_EQ(message.readOffset(), message.size());
}

TEST(VoxelVolumeMessage, DecodedStorageOutlivesSourceMessage)
{
	const auto source = makeAsymmetricVolume();
	Voxel::Volume destination;

	{
		spk::Message message;
		message << source;
		message >> destination;
	}

	expectVolumesEqual(destination, source);
	EXPECT_NE(destination.cells().data(), source.cells().data());
}

TEST(VoxelVolumeMessage, TruncatedDimensionsAreRejectedWithoutChangingDestination)
{
	spk::Message::Storage payload(sizeof(spk::Vector3UInt) - 1u);
	spk::Message message(0u, std::move(payload));

	expectDecodeFailurePreservesDestination(message);
	EXPECT_EQ(message.readOffset(), 0u);
}

TEST(VoxelVolumeMessage, TruncatedUnitSizeIsRejectedWithoutChangingDestination)
{
	spk::Message message;
	const spk::Vector3UInt dimensions{1, 1, 1};
	message << dimensions;

	const std::byte partialUnitSize{};
	message.append(&partialUnitSize, 1u);

	expectDecodeFailurePreservesDestination(message);
	EXPECT_EQ(message.readOffset(), sizeof(spk::Vector3UInt));
}

TEST(VoxelVolumeMessage, TruncatedCellBlockIsRejectedBeforeDestinationReplacement)
{
	spk::Message message = metadataMessage({2, 1, 1}, 1.0f);
	const Voxel::Cell firstCell(7u);
	message.append(&firstCell, sizeof(firstCell));

	expectDecodeFailurePreservesDestination(message);
	EXPECT_EQ(
		message.readOffset(),
		sizeof(spk::Vector3UInt) + sizeof(Voxel::Volume::UnitSize));
}

TEST(VoxelVolumeMessage, MixedZeroDimensionsAreRejected)
{
	const std::array<spk::Vector3UInt, 3> invalidDimensions = {
		spk::Vector3UInt{0, 1, 1},
		spk::Vector3UInt{1, 0, 1},
		spk::Vector3UInt{1, 1, 0}};

	for (const auto &dimensions : invalidDimensions)
	{
		spk::Message message = metadataMessage(dimensions, 1.0f);
		expectDecodeFailurePreservesDestination(message);
	}
}

TEST(VoxelVolumeMessage, EmptyDimensionsRequireZeroUnitSize)
{
	spk::Message message = metadataMessage({0, 0, 0}, 1.0f);

	expectDecodeFailurePreservesDestination(message);
}

TEST(VoxelVolumeMessage, NonEmptyDimensionsRejectInvalidUnitSizes)
{
	const std::array<Voxel::Volume::UnitSize, 5> invalidUnitSizes = {
		0.0f,
		-1.0f,
		std::numeric_limits<float>::quiet_NaN(),
		std::numeric_limits<float>::infinity(),
		-std::numeric_limits<float>::infinity()};

	for (const auto unitSize : invalidUnitSizes)
	{
		spk::Message message = metadataMessage({1, 1, 1}, unitSize);
		expectDecodeFailurePreservesDestination(message);
	}
}

TEST(VoxelVolumeMessage, OverflowingDimensionProductIsRejectedBeforeAllocation)
{
	constexpr auto maximum = std::numeric_limits<std::uint32_t>::max();
	spk::Message message = metadataMessage({maximum, maximum, maximum}, 1.0f);

	expectDecodeFailurePreservesDestination(message);
}

TEST(VoxelVolumeMessage, OverflowingCellByteSizeIsRejectedBeforeAllocation)
{
	constexpr auto maximum = std::numeric_limits<std::uint32_t>::max();
	spk::Message message = metadataMessage({maximum, maximum, 1u}, 1.0f);

	expectDecodeFailurePreservesDestination(message);
}

TEST(VoxelVolumeMessage, MissingHugeCellBlockIsRejectedBeforeAllocation)
{
	constexpr auto maximum = std::numeric_limits<std::uint32_t>::max();
	spk::Message message = metadataMessage({maximum, 1u, 1u}, 1.0f);

	expectDecodeFailurePreservesDestination(message);
}

TEST(VoxelVolumeMessage, DecodeReusesDestinationBufferFromTheSamePoolClass)
{
	Voxel::Volume::Builder destinationBuilder({10, 10, 50}, 1.0f);
	Voxel::Volume destination = std::move(destinationBuilder).build();
	const Voxel::Cell *originalData = destination.cells().data();

	Voxel::Volume::Builder sourceBuilder({10, 10, 70}, 0.5f);
	ASSERT_TRUE(sourceBuilder.set({9, 9, 69}, Voxel::Cell(123u)));
	const Voxel::Volume source = std::move(sourceBuilder).build();

	spk::Message message;
	message << source;
	message >> destination;

	EXPECT_EQ(destination.cells().data(), originalData);
	expectVolumesEqual(destination, source);
}

TEST(VoxelVolumeMessage, SamePoolTruncationDoesNotMutateDestinationBuffer)
{
	Voxel::Volume::Builder destinationBuilder({10, 10, 50}, 1.0f);
	ASSERT_TRUE(destinationBuilder.set({9, 9, 49}, Voxel::Cell(456u)));
	Voxel::Volume destination = std::move(destinationBuilder).build();
	const Voxel::Volume expected(destination);
	const Voxel::Cell *originalData = destination.cells().data();

	spk::Message message = metadataMessage({10, 10, 70}, 0.5f);
	const Voxel::Cell partialCell(7u);
	message.append(&partialCell, sizeof(partialCell));

	EXPECT_THROW((message >> destination), spk::Exception);
	EXPECT_EQ(destination.cells().data(), originalData);
	expectVolumesEqual(destination, expected);
}

TEST(VoxelVolumeMessage, DecodeReplacesDestinationBufferWhenPoolClassChanges)
{
	Voxel::Volume::Builder destinationBuilder({10, 10, 30}, 1.0f);
	Voxel::Volume destination = std::move(destinationBuilder).build();
	const Voxel::Cell *originalData = destination.cells().data();

	Voxel::Volume::Builder sourceBuilder({10, 10, 50}, 0.5f);
	ASSERT_TRUE(sourceBuilder.set({9, 9, 49}, Voxel::Cell(321u)));
	const Voxel::Volume source = std::move(sourceBuilder).build();

	spk::Message message;
	message << source;
	message >> destination;

	EXPECT_NE(destination.cells().data(), originalData);
	expectVolumesEqual(destination, source);
}

TEST(VoxelVolumeMessage, RepeatedRoundTripsPreserveLogicalState)
{
	const auto source = makeAsymmetricVolume();
	Voxel::Volume current(source);

	for (int iteration = 0; iteration < 3; ++iteration)
	{
		spk::Message message;
		message << current;

		Voxel::Volume next;
		message >> next;
		expectVolumesEqual(next, source);
		current = std::move(next);
	}
}
