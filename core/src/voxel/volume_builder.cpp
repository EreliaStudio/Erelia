#include "erelia/core/voxel/volume_builder.hpp"

#include <cmath>
#include <limits>
#include <map>
#include <span>
#include <utility>

#include <container/pool.hpp>
#include <exception.hpp>

#include "erelia/core/chunk.hpp"
#include "volume_content.hpp"

namespace
{
	using CellBuffer = std::vector<Voxel::Cell>;
	using CellBufferPool = spk::Pool<CellBuffer>;
	using CellBufferLease = CellBufferPool::Lease;

	constexpr std::size_t ChunkCellCount =
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent);

	[[nodiscard]] CellBufferPool::Factory makeCellBufferFactory(std::size_t capacity)
	{
		return [capacity]() {
			auto *buffer = new CellBuffer();
			buffer->reserve(capacity);
			return buffer;
		};
	}

	CellBufferPool chunkCellBufferPool(makeCellBufferFactory(ChunkCellCount));
	std::map<std::size_t, CellBufferPool> cellBufferPools;

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

	[[nodiscard]] bool areChunkDimensions(const spk::Vector3UInt &dimensions) noexcept
	{
		return dimensions.x == Chunk::Extent && dimensions.y == Chunk::Extent && dimensions.z == Chunk::Extent;
	}

	[[nodiscard]] CellBufferPool &cellBufferPoolFor(
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

	[[nodiscard]] CellBufferLease obtainEmptyCellBuffer(
		const spk::Vector3UInt &dimensions,
		std::size_t expectedSize)
	{
		return cellBufferPoolFor(dimensions, expectedSize).obtain([](CellBuffer &buffer, std::size_t size) {
			buffer.clear();
			buffer.resize(size);
		},
			expectedSize);
	}

	[[nodiscard]] CellBufferLease obtainCopiedCellBuffer(
		const spk::Vector3UInt &dimensions,
		std::span<const Voxel::Cell> source)
	{
		return cellBufferPoolFor(dimensions, source.size()).obtain([](CellBuffer &buffer, std::span<const Voxel::Cell> cells) {
			buffer.assign(cells.begin(), cells.end());
		},
			source);
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
		UnitSize unitSize)
	{
		const auto expectedCellCount = cellCount(dimensions);
		const auto validUnitSize = validatedUnitSize(unitSize);
		auto cells = obtainEmptyCellBuffer(dimensions, expectedCellCount);

		_content = std::make_shared<Content>(
			dimensions,
			validUnitSize,
			std::move(cells));
	}

	Volume::Builder::Builder(Volume &&volume)
	{
		auto sourceContent = std::move(volume._content);
		if (sourceContent == nullptr)
		{
			return;
		}

		if (sourceContent.use_count() == 1)
		{
			_content = std::move(sourceContent);
			return;
		}

		const auto sourceCells = std::span<const Cell>(
			sourceContent->cells->data(),
			sourceContent->cells->size());
		auto cells = obtainCopiedCellBuffer(sourceContent->dimensions, sourceCells);

		_content = std::make_shared<Content>(
			sourceContent->dimensions,
			sourceContent->unitSize,
			std::move(cells));
	}

	bool Volume::Builder::set(const LocalCoordinate &coordinate, Cell value)
	{
		const auto dimensionsValue =
			_content != nullptr ? _content->dimensions : spk::Vector3UInt{};
		const auto index = checkedIndex(dimensionsValue, coordinate);
		auto &cell = (*_content->cells)[index];

		if (cell.packed() == value.packed())
		{
			return false;
		}

		cell = value;
		return true;
	}

	Volume Volume::Builder::build() && noexcept
	{
		return Volume(std::move(_content));
	}
}
