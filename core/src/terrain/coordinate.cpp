#include "erelia/core/chunk.hpp"

namespace
{
	[[nodiscard]] constexpr std::int32_t floorDivideByChunkExtent(std::int32_t value) noexcept
	{
		const std::int32_t quotient = value / Chunk::Extent;
		const std::int32_t remainder = value % Chunk::Extent;

		return remainder < 0 ? quotient - 1 : quotient;
	}

	[[nodiscard]] constexpr std::int32_t floorModuloByChunkExtent(std::int32_t value) noexcept
	{
		const std::int32_t remainder = value % Chunk::Extent;

		return remainder < 0 ? remainder + Chunk::Extent : remainder;
	}
}

Chunk::Coordinate Chunk::toCoordinate(const Voxel::Cell::Coordinate &globalCell) noexcept
{
	return {
		floorDivideByChunkExtent(globalCell.x),
		floorDivideByChunkExtent(globalCell.y),
		floorDivideByChunkExtent(globalCell.z)};
}

Voxel::Volume::LocalCoordinate Chunk::toLocalCoordinate(const Voxel::Cell::Coordinate &globalCell) noexcept
{
	return {
		floorModuloByChunkExtent(globalCell.x),
		floorModuloByChunkExtent(globalCell.y),
		floorModuloByChunkExtent(globalCell.z)};
}
