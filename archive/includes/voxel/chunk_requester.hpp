#pragma once

#include <cstdint>
#include <set>

#include "voxel/chunk_collection.hpp"

namespace voxel
{
	class Chunk::Requester final
	{
	private:
		Collection &_chunks;
		std::int32_t _horizontalRadius;
		std::int32_t _verticalRadius;
		std::set<Coordinate> _requested;

	public:
		Requester(Collection &chunks, std::int32_t horizontalRadius, std::int32_t verticalRadius = 0);

		void focus(Coordinate coordinate);
	};
}
