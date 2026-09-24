#pragma once

#include "voxel/voxel_cell.hpp"
#include "voxel/voxel_shape.hpp"

namespace voxel::Voxel
{
	[[nodiscard]] Side oppositeSide(Side side) noexcept;
	[[nodiscard]] spk::Vector3Int sideOffset(Side side) noexcept;
	[[nodiscard]] spk::Vector3 transformPosition(spk::Vector3 position, Cell cell) noexcept;
	[[nodiscard]] spk::Vector3 transformNormal(spk::Vector3 normal, Cell cell) noexcept;
	[[nodiscard]] Side transformSide(Side side, Cell cell) noexcept;
	[[nodiscard]] Shape::Polygon transformPolygon(const Shape::Polygon &polygon, Cell cell);
}
