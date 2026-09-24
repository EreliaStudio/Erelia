#include "erelia/core/voxel/volume.hpp"

#include <bit>
#include <limits>
#include <map>

#include <exception.hpp>

namespace
{
	using Buffer = Voxel::Volume::Buffer;

	[[nodiscard]] std::size_t cellBufferPoolCapacity(std::size_t cellCount)
	{
		if (cellCount == 0)
		{
			throw spk::Exception("Voxel::Volume Cell buffer capacity must be strictly positive");
		}

		constexpr std::size_t highestPowerOfTwo =
			std::size_t{1} << (std::numeric_limits<std::size_t>::digits - 1);

		if (cellCount > highestPowerOfTwo)
		{
			throw spk::Exception("Voxel::Volume Cell buffer capacity exceeds the largest representable power-of-two size class");
		}

		return std::bit_ceil(cellCount);
	}

	class CellArrayPool : public Buffer::Pool
	{
	public:
		explicit CellArrayPool(std::size_t capacity) :
			Buffer::Pool([capacity]() {
				auto *buffer = new Buffer();
				buffer->reserve(capacity);
				return buffer;
			})
		{
		}
	};

	class CellArrayCollection
	{
	private:
		std::map<std::size_t, CellArrayPool> _collection;

	public:
		[[nodiscard]] CellArrayPool &operator[](std::size_t cellCount)
		{
			const std::size_t capacity = cellBufferPoolCapacity(cellCount);
			return _collection.try_emplace(capacity, capacity).first->second;
		}
	};

	CellArrayCollection cellArrayCollection;

	void resetCellBuffer(Buffer &buffer, std::size_t size)
	{
		buffer.clear();
		buffer.resize(size);
	}
}

namespace Voxel
{
	Volume::Buffer::Lease Volume::obtainCellBuffer(std::size_t expectedSize)
	{
		return cellArrayCollection[expectedSize].obtain(resetCellBuffer, expectedSize);
	}

	bool Volume::canReuseCellBuffer(
		const Buffer::Lease &cells,
		std::size_t expectedSize)
	{
		if (!cells)
		{
			return false;
		}

		return cellBufferPoolCapacity(cells->size()) ==
			   cellBufferPoolCapacity(expectedSize);
	}
}
