#include "voxel/chunk.hpp"

#include <exception.hpp>

namespace voxel
{
	namespace
	{
		std::int32_t floorDiv(std::int32_t value, std::int32_t divisor) noexcept
		{
			const auto quotient = value / divisor;
			const auto remainder = value % divisor;
			return quotient - (remainder < 0);
		}

		std::int32_t floorMod(std::int32_t value, std::int32_t divisor) noexcept
		{
			const auto remainder = value % divisor;
			return remainder < 0 ? remainder + divisor : remainder;
		}
	}

	Chunk::Chunk(Coordinate coordinate) :
		VoxelVolume({Size, Size, Size}, 1.0f),
		_coordinate(coordinate)
	{
	}

	bool Chunk::contains(spk::Vector3Int local) noexcept
	{
		return local.x >= 0 && local.y >= 0 && local.z >= 0 && local.x < Size && local.y < Size && local.z < Size;
	}

	std::size_t Chunk::index(spk::Vector3Int local)
	{
		if (!contains(local))
		{
			throw spk::Exception("chunk local coordinate is out of range");
		}
		return static_cast<std::size_t>((local.y * Size + local.z) * Size + local.x);
	}

	Chunk::Coordinate Chunk::worldToChunk(spk::Vector3Int world) noexcept
	{
		return {floorDiv(world.x, Size), floorDiv(world.y, Size), floorDiv(world.z, Size)};
	}

	spk::Vector3Int Chunk::worldToLocal(spk::Vector3Int world) noexcept
	{
		return {floorMod(world.x, Size), floorMod(world.y, Size), floorMod(world.z, Size)};
	}

	spk::Vector3Int Chunk::worldOrigin(Coordinate coordinate) noexcept
	{
		return {coordinate.x * Size, coordinate.y * Size, coordinate.z * Size};
	}

}
