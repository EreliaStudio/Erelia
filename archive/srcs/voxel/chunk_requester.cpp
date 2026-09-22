#include "voxel/chunk_requester.hpp"

#include <exception.hpp>
#include <utility>

namespace voxel
{
	Chunk::Requester::Requester(Collection &chunks, std::int32_t horizontalRadius, std::int32_t verticalRadius) :
		_chunks(chunks),
		_horizontalRadius(horizontalRadius),
		_verticalRadius(verticalRadius)
	{
		if (horizontalRadius < 0 || verticalRadius < 0)
		{
			throw spk::Exception("chunk request radius cannot be negative");
		}
	}

	void Chunk::Requester::focus(Coordinate coordinate)
	{
		std::set<Coordinate> desired;
		for (int y = -_verticalRadius; y <= _verticalRadius; ++y)
		{
			for (int z = -_horizontalRadius; z <= _horizontalRadius; ++z)
			{
				for (int x = -_horizontalRadius; x <= _horizontalRadius; ++x)
				{
					desired.insert({coordinate.x + x, coordinate.y + y, coordinate.z + z});
				}
			}
		}
		for (auto requested : _requested)
		{
			if (!desired.contains(requested))
			{
				_chunks.remove(requested);
			}
		}
		for (auto requested : desired)
		{
			_chunks.request(requested);
		}
		_requested = std::move(desired);
	}
}
