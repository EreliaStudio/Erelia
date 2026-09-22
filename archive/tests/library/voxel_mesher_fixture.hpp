#pragma once

#include <memory>

#include "voxel/voxel_model.hpp"

namespace playground_test
{
	inline constexpr std::size_t MesherFixtureCellCount = 15;
	inline constexpr std::size_t MesherFixtureVertexCount = 288;
	inline constexpr std::size_t MesherFixtureIndexCount = 432;

	[[nodiscard]] std::unique_ptr<voxel::VoxelModel> loadVoxelMesherModel();
	[[nodiscard]] std::unique_ptr<voxel::VoxelVolume> makeVoxelMesherVolume();
}
