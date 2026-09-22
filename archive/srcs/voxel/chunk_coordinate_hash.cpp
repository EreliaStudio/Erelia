#include "voxel/chunk_coordinate_hash.hpp"

#include <cstdint>

namespace voxel
{
	std::size_t Chunk::CoordinateHash::operator()(const Coordinate &value) const noexcept
	{
		std::size_t hash = static_cast<std::uint32_t>(value.x) * 0x9E3779B1u;
		hash ^= static_cast<std::uint32_t>(value.y) + 0x9e3779b9 + (hash << 6) + (hash >> 2);
		hash ^= static_cast<std::uint32_t>(value.z) + 0x9e3779b9 + (hash << 6) + (hash >> 2);
		return hash;
	}
}
