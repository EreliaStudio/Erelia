#pragma once

#include <cstddef>

#include "voxel/chunk.hpp"

namespace voxel
{
	struct Chunk::CoordinateHash
	{
		std::size_t operator()(const Coordinate &value) const noexcept;
	};
}
