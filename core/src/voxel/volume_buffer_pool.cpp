#include "volume_buffer_pool.hpp"

#include <bit>
#include <limits>
#include <map>

#include <exception.hpp>

namespace
{
	using Buffer = Voxel::Volume::Buffer;

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

		[[nodiscard]] static std::size_t _nextPowerOfTwo(std::size_t capacity)
		{
			if (capacity == 0)
			{
				throw spk::Exception("Voxel::Volume Cell buffer capacity must be strictly positive");
			}

			constexpr std::size_t highestPowerOfTwo =
				std::size_t{1} << (std::numeric_limits<std::size_t>::digits - 1);

			if (capacity > highestPowerOfTwo)
			{
				throw spk::Exception("Voxel::Volume Cell buffer capacity exceeds the largest representable power-of-two size class");
			}

			return std::bit_ceil(capacity);
		}

	public:
		[[nodiscard]] CellArrayPool &operator[](std::size_t capacity)
		{
			auto iterator = _collection.lower_bound(capacity);
			if (iterator != _collection.end())
			{
				return iterator->second;
			}

			const std::size_t bound = _nextPowerOfTwo(capacity);
			return _collection.try_emplace(bound, bound).first->second;
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
	Volume::Buffer::Lease obtainCellBuffer(std::size_t expectedSize)
	{
		return cellArrayCollection[expectedSize].obtain(resetCellBuffer, expectedSize);
	}
}
