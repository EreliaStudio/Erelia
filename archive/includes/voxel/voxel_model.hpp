#pragma once

#include <container/json/reader.hpp>

#include "voxel/voxel_volume.hpp"

namespace voxel
{
	class VoxelModel final : public VoxelVolume
	{
	public:
		VoxelModel(spk::Vector3UInt dimensions, float voxelSize);
		explicit VoxelModel(const spk::JSON::Reader &reader);
	};
}
