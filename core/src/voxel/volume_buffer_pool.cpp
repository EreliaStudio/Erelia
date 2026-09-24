#include "volume_buffer_pool.hpp"

#include <map>

namespace
{
	using Buffer = Voxel::Volume::Buffer;

	[[nodiscard]] Buffer::Pool::Factory makeCellBufferFactory(std::size_t capacity)
	{
		return [capacity]() {
			auto *buffer = new Buffer();
			buffer->reserve(capacity);
			return buffer;
		};
	}

	std::map<std::size_t, Buffer::Pool> cellBufferPools;

	[[nodiscard]] Buffer::Pool &cellBufferPoolFor(std::size_t expectedSize)
	{
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
}

namespace Voxel
{
	Volume::Buffer::Lease obtainCellBuffer(std::size_t expectedSize)
	{
		return cellBufferPoolFor(expectedSize).obtain(resetCellBuffer, expectedSize);
	}
}
