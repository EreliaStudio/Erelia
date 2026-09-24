#pragma once

#include "voxel/voxel_definition.hpp"
#include "voxel/voxel_mesh.hpp"
#include "voxel/voxel_volume.hpp"
#include "voxel/palette.hpp"

namespace voxel
{
	class MaterialResolver
	{
	public:
		struct Context
		{
			const VoxelVolume &volume;
			const Voxel::Definition &definition;
			const Voxel::Shape::Polygon &polygon;
			spk::Vector3Int coordinate;
			Voxel::Cell cell;
		};

		virtual ~MaterialResolver() = default;
		[[nodiscard]] virtual Palette::ElementIndex resolve(const Context &context) const = 0;
	};
}
