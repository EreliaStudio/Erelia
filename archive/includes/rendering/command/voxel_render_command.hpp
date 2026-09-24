#pragma once

#include <sparkle>

#include "voxel/palette.hpp"
#include "voxel/voxel_mesh.hpp"

namespace voxel
{
	class VoxelRenderCommand final : public spk::RenderCommand
	{
	private:
		static constexpr std::size_t ModelBindingPoint = 4;

		VoxelMesh _mesh;
		Palette _palette;
		spk::Matrix4x4 _modelMatrix;

		[[nodiscard]] static spk::Program &_sharedProgram();
		[[nodiscard]] static spk::UniformBuffer &_sharedModelBuffer();

	public:
		VoxelRenderCommand(const Palette &palette, VoxelMesh mesh, spk::Matrix4x4 modelMatrix);

		void execute(spk::RenderContext &context) const override;
	};
}
