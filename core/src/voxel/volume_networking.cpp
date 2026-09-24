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
		const spk::Message &message,
		std::size_t cellBytes)
	{
		if (message.readOffset() > message.size())
		{
			throw spk::Exception("Voxel::Volume Message read offset is outside the payload");
		}

		const std::size_t remainingBytes = message.size() - message.readOffset();
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
		message >> *this;
	}

	spk::Message &operator<<(
		spk::Message &message,
		const Volume &volume)
	{
		const SerializedVolumeLayout layout =
			validatedSerializedVolumeLayout(volume);

		message << volume._dimensions;
		message << volume._unitSize;
		message.append(volume.cells().data(), layout.cellBytes);

		return message;
	}

	const spk::Message &operator>>(
		const spk::Message &message,
		Volume &volume)
	{
		spk::Vector3UInt dimensions{};
		Volume::UnitSize unitSize = 0.0f;

		message >> dimensions;
		message >> unitSize;

		const SerializedVolumeLayout layout =
			validatedSerializedVolumeLayout(dimensions, unitSize);
		validateCellBytesAvailable(message, layout.cellBytes);

		if (layout.cellCount == 0)
		{
			volume = Volume();
			return message;
		}

		if (Volume::canReuseCellBuffer(volume._cells, layout.cellCount))
		{
			volume._cells->resize(layout.cellCount);
			message.pull(volume._cells->data(), layout.cellBytes);
			volume._dimensions = dimensions;
			volume._unitSize = unitSize;
			return message;
		}

		Volume::Buffer::Lease cells =
			Volume::obtainCellBuffer(layout.cellCount);
		message.pull(cells->data(), layout.cellBytes);

		Volume decoded(
			dimensions,
			unitSize,
			std::move(cells));
		volume = std::move(decoded);

		return message;
	}
}
