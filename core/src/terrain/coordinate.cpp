#include "erelia/core/terrain/coordinate.hpp"

namespace
{
	[[nodiscard]] constexpr std::int32_t floorDivideByChunkExtent(std::int32_t value) noexcept
	{
		const std::int32_t quotient = value / core::terrain::chunkExtent;
		const std::int32_t remainder = value % core::terrain::chunkExtent;

		return remainder < 0 ? quotient - 1 : quotient;
	}

	[[nodiscard]] constexpr std::int32_t floorModuloByChunkExtent(std::int32_t value) noexcept
	{
		const std::int32_t remainder = value % core::terrain::chunkExtent;

		return remainder < 0 ? remainder + core::terrain::chunkExtent : remainder;
	}
}

spk::Vector3Int core::terrain::toChunkCoordinate(const spk::Vector3Int &globalCell) noexcept
{
	return {
		floorDivideByChunkExtent(globalCell.x),
		floorDivideByChunkExtent(globalCell.y),
		floorDivideByChunkExtent(globalCell.z)};
}

spk::Vector3Int core::terrain::toLocalCoordinate(const spk::Vector3Int &globalCell) noexcept
{
	return {
		floorModuloByChunkExtent(globalCell.x),
		floorModuloByChunkExtent(globalCell.y),
		floorModuloByChunkExtent(globalCell.z)};
}
