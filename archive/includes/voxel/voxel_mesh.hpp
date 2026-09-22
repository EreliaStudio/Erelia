#pragma once

#include <cstdint>

#include <sparkle>
#include "geometry/mesh.hpp"

namespace voxel
{
	struct VoxelVertex
	{
		using PaletteElementIndex = std::uint32_t;

		spk::Vector3 position;
		spk::Vector3 normal;
		PaletteElementIndex paletteElementIndex = 0;
		bool operator==(const VoxelVertex &) const noexcept = default;
	};

	class VoxelMesh final : public spk::Mesh<VoxelVertex>
	{
	public:
		class Builder final : public spk::Mesh<VoxelVertex>::Builder<VoxelMesh>
		{
		private:
			void _setupAttributes(spk::VertexBuffer &vertexBuffer) override;
		};
	};
}
