#include "erelia/core/voxel/volume_builder.hpp"

#include <cmath>
#include <cstdint>
#include <limits>
#include <map>
#include <type_traits>
#include <utility>

#include <exception.hpp>
#include <network/message.hpp>

#include "erelia/core/chunk.hpp"

namespace
{
	using Buffer = Voxel::Volume::Buffer;

	static_assert(std::is_trivially_copyable_v<spk::Vector3UInt>);
	static_assert(sizeof(spk::Vector3UInt) == 3 * sizeof(std::uint32_t));
	static_assert(std::is_trivially_copyable_v<Voxel::Volume::UnitSize>);
	static_assert(std::is_trivially_copyable_v<Voxel::Cell>);
	static_assert(sizeof(Voxel::Cell) == sizeof(Voxel::Cell::PackedType));

	constexpr std::size_t ChunkCellCount =
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent);

	struct SerializedVolumeLayout
	{
		std::size_t cellCount = 0;
		std::size_t cellBytes = 0;
	};

	[[nodiscard]] Buffer::Pool::Factory makeCellBufferFactory(std::size_t capacity)
	{
		return [capacity]() {
			auto *buffer = new Buffer();
			buffer->reserve(capacity);
			return buffer;
		};
	}

	Buffer::Pool chunkCellBufferPool(makeCellBufferFactory(ChunkCellCount));
	std::map<std::size_t, Buffer::Pool> cellBufferPools;

	[[nodiscard]] std::size_t cellCount(const spk::Vector3UInt &dimensions)
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

	[[nodiscard]] Voxel::Volume::UnitSize validatedUnitSize(Voxel::Volume::UnitSize unitSize)
	{
		if (!std::isfinite(unitSize) || unitSize <= 0.0f)
		{
			throw spk::Exception("Voxel::Volume unit size must be finite and strictly positive");
		}

		return unitSize;
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

		static_cast<void>(validatedUnitSize(unitSize));

		const std::size_t expectedCellCount = cellCount(dimensions);
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

	[[nodiscard]] bool areChunkDimensions(const spk::Vector3UInt &dimensions) noexcept
	{
		return dimensions.x == Chunk::Extent && dimensions.y == Chunk::Extent && dimensions.z == Chunk::Extent;
	}

	[[nodiscard]] Buffer::Pool &cellBufferPoolFor(
		const spk::Vector3UInt &dimensions,
		std::size_t expectedSize)
	{
		if (areChunkDimensions(dimensions))
		{
			return chunkCellBufferPool;
		}

		auto iterator = cellBufferPools.lower_bound(expectedSize);
		if (iterator != cellBufferPools.end())
		{
			return iterator->second;
		}

		auto [insertedIterator, inserted] = cellBufferPools.try_emplace(
			expectedSize,
			makeCellBufferFactory(expectedSize));
		static_cast<void>(inserted);

		return insertedIterator->second;
	}

	void resetCellBuffer(Buffer &buffer, std::size_t size)
	{
		buffer.clear();
		buffer.resize(size);
	}

	[[nodiscard]] Buffer::Lease obtainEmptyCellBuffer(
		const spk::Vector3UInt &dimensions,
		std::size_t expectedSize)
	{
		return cellBufferPoolFor(dimensions, expectedSize).obtain(resetCellBuffer, expectedSize);
	}

	[[nodiscard]] std::size_t checkedIndex(
		const spk::Vector3UInt &dimensions,
		const Voxel::Volume::LocalCoordinate &coordinate)
	{
		const bool contained =
			coordinate.x >= 0 &&
			coordinate.y >= 0 &&
			coordinate.z >= 0 &&
			static_cast<std::uint32_t>(coordinate.x) < dimensions.x &&
			static_cast<std::uint32_t>(coordinate.y) < dimensions.y &&
			static_cast<std::uint32_t>(coordinate.z) < dimensions.z;

		if (!contained)
		{
			throw spk::Exception("Voxel::Volume::Builder coordinate is out of range");
		}

		const auto x = static_cast<std::size_t>(coordinate.x);
		const auto y = static_cast<std::size_t>(coordinate.y);
		const auto z = static_cast<std::size_t>(coordinate.z);
		const auto sizeX = static_cast<std::size_t>(dimensions.x);
		const auto sizeY = static_cast<std::size_t>(dimensions.y);

		return y + sizeY * (x + sizeX * z);
	}
}

namespace Voxel
{
	Volume::Builder::Builder(
		const spk::Vector3UInt &dimensions,
		UnitSize unitSize) :
		_dimensions(dimensions),
		_unitSize(validatedUnitSize(unitSize)),
		_cells(obtainEmptyCellBuffer(dimensions, cellCount(dimensions)))
	{
	}

	Volume::Builder::Builder(Volume &&volume) noexcept :
		_dimensions(std::exchange(volume._dimensions, {})),
		_unitSize(std::exchange(volume._unitSize, 0.0f)),
		_cells(std::move(volume._cells))
	{
	}

	bool Volume::Builder::set(const LocalCoordinate &coordinate, Cell value)
	{
		const auto index = checkedIndex(_dimensions, coordinate);
		auto &cell = (*_cells)[index];

		if (cell.packed() == value.packed())
		{
			return false;
		}

		cell = value;
		return true;
	}

	Volume Volume::Builder::build() && noexcept
	{
		return Volume(
			std::exchange(_dimensions, {}),
			std::exchange(_unitSize, 0.0f),
			std::move(_cells));
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

		Volume::Builder builder(dimensions, unitSize);
		message.pull(builder._cells->data(), layout.cellBytes);

		volume = std::move(builder).build();
		return message;
	}
}
