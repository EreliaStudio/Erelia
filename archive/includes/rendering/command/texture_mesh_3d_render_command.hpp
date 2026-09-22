#pragma once


#include "math/matrix.hpp"

#include "geometry/texture_mesh_3d.hpp"
#include "graphics/opengl/sampler.hpp"
#include "rendering/render_command.hpp"

namespace spk
{
	class Program;
	class Texture;
	class UniformBuffer;

	class TextureMesh3DRenderCommand final : public RenderCommand
	{
	private:
		static constexpr std::size_t ModelBindingPoint = 4;
		static constexpr std::size_t TextureBindingPoint = 0;

		const Texture *_texture;
		TextureMesh3D _mesh;
		Matrix4x4 _modelMatrix;
		Sampler _sampler;

		static Program &_sharedProgram();
		static UniformBuffer &_sharedModelBuffer();

	public:
		TextureMesh3DRenderCommand(const Texture *texture, TextureMesh3D mesh, Matrix4x4 modelMatrix);

		void execute(RenderContext &context) const override;
	};
}
