#include "erelia/core/voxel/volume.hpp"

#include <cmath>
#include <cstdint>
#include <limits>
#include <type_traits>
#include <utility>

#include <exception.hpp>

namespace
{
	static_assert(std::is_trivially_copyable_v<spk::Vector3UInt>);
	static_assert(sizeof(spk::Vector3UInt) == 3 * sizeof(std::uint32_t));
	static_assert(std::is_trivially_copyable_v<Voxel::Volume::UnitSize>);
	static_assert(std::is_trivially_copyable_v<Voxel::Cell>);
	static_assert(sizeof(Voxel::Cell) == sizeof(Voxel::Cell::PackedType));

	struct SerializedVolumeLayout
	{
		std::size_t cellCount = 0;
		std::size_t cellBytes = 0;
	};

	[[nodiscard]] std::size_t serializedCellCount(const spk::Vector3UInt &dimensions)
	{
		if (dimensions.x == 0 || dimensions.y == 0 || dimensions.z == 0)
		{
			throw spk::Exception("Voxel::Volume dimensions must be strictly positive");
		}

		constexpr auto maximum = std::numeric_limits<std::size_t>::max();
		const auto sizeX = static_cast<std::size_t>(dimensions.x);
		const auto sizeY = static_cast<std::size_t>(dimensions.y);
		const auto sizeZ = static_cast<std::size_t>(dimensions.z);

		if (sizeY > maximum / sizeX || sizeZ > maximum / (sizeX * sizeY))
		{
			throw spk::Exception("Voxel::Volume cell count exceeds std::size_t capacity");
		}

		return sizeX * sizeY * sizeZ;
	}

	[[nodiscard]] SerializedVolumeLayout validatedSerializedVolumeLayout(
		const spk::Vector3UInt &dimensions,
		Voxel::Volume::UnitSize unitSize)
	{
		const bool emptyDimensions =
			dimensions.x == 0 &&
			dimensions.y == 0 &&
			dimensions.z == 0;

		if (emptyDimensions)
		{
			if (unitSize != 0.0f)
			{
				throw spk::Exception("Empty Voxel::Volume must have a zero unit size");
			}

			return {};
		}

		if (dimensions.x == 0 || dimensions.y == 0 || dimensions.z == 0)
		{
			throw spk::Exception("Voxel::Volume dimensions must either all be zero or all be strictly positive");
		}

		if (!std::isfinite(unitSize) || unitSize <= 0.0f)
		{
			throw spk::Exception("Voxel::Volume unit size must be finite and strictly positive");
		}

		const std::size_t expectedCellCount = serializedCellCount(dimensions);
		constexpr auto maximum = std::numeric_limits<std::size_t>::max();

		if (expectedCellCount > maximum / sizeof(Voxel::Cell))
		{
			throw spk::Exception("Voxel::Volume serialized Cell block exceeds std::size_t capacity");
		}

		return {
			expectedCellCount,
			expectedCellCount * sizeof(Voxel::Cell)};
	}

	[[nodiscard]] SerializedVolumeLayout validatedSerializedVolumeLayout(
		const Voxel::Volume &volume)
	{
		const auto result = validatedSerializedVolumeLayout(
			volume.dimensions(),
			volume.unitSize());

		if (volume.cells().size() != result.cellCount)
		{
			throw spk::Exception("Voxel::Volume Cell storage does not match its dimensions");
		}

		return result;
	}

	void validateCellBytesAvailable(
		const spk::Message::Reader &reader,
		std::size_t cellBytes)
	{
		if (reader.readOffset() > reader.size())
		{
			throw spk::Exception("Voxel::Volume Message read offset is outside the payload");
		}

		const std::size_t remainingBytes =
			reader.size() - reader.readOffset();
		if (cellBytes > remainingBytes)
		{
			throw spk::Exception("Voxel::Volume Message does not contain the complete Cell block");
		}
	}
}

namespace Voxel
{
	Volume::Volume(const spk::Message &message)
	{
		auto reader = message.reader();
		reader >> *this;
	}

	spk::Message::Writer &operator<<(
		spk::Message::Writer &writer,
		const Volume &volume)
	{
		const SerializedVolumeLayout layout =
			validatedSerializedVolumeLayout(volume);

		writer << volume._dimensions;
		writer << volume._unitSize;
		writer.append(volume.cells().data(), layout.cellBytes);

		return writer;
	}

	const spk::Message::Reader &operator>>(
		const spk::Message::Reader &reader,
		Volume &volume)
	{
		spk::Vector3UInt dimensions{};
		Volume::UnitSize unitSize = 0.0f;

		reader >> dimensions;
		reader >> unitSize;

		const SerializedVolumeLayout layout =
			validatedSerializedVolumeLayout(dimensions, unitSize);
		validateCellBytesAvailable(reader, layout.cellBytes);

		if (layout.cellCount == 0)
		{
			volume = Volume();
			return reader;
		}

		Volume::Buffer::Lease cells =
			Volume::obtainCellBuffer(layout.cellCount);
		reader.pull(cells->data(), layout.cellBytes);

		Volume decoded(
			dimensions,
			unitSize,
			std::move(cells));
		volume = std::move(decoded);

		return reader;
	}
}
