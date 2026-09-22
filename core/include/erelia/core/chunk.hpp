#pragma once

#include <cstdint>

#include "erelia/core/voxel/cell.hpp"
#include "erelia/core/voxel/volume.hpp"

struct Chunk : public Voxel::Volume
{
	using Coordinate = spk::Vector3Int;

	inline static constexpr std::int32_t Extent = 16;
	inline static constexpr VoxelSize CellSize = 1.0F;

	[[nodiscard]] static Coordinate toCoordinate(const Voxel::Cell::Coordinate &globalCell) noexcept;
	[[nodiscard]] static Voxel::Volume::LocalCoordinate toLocalCoordinate(
		const Voxel::Cell::Coordinate &globalCell) noexcept;
};
