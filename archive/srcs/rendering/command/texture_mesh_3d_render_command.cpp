#include "rendering/command/texture_mesh_3d_render_command.hpp"

#include <memory>
#include <exception.hpp>

#include "graphics/opengl/program.hpp"
#include "graphics/opengl/texture.hpp"
#include "graphics/opengl/uniform_buffer.hpp"

#include "rendering/command/camera_ubo_render_command.hpp"

#include "sparkle.hpp"

namespace spk
{
	Program &TextureMesh3DRenderCommand::_sharedProgram()
	{
		static auto program = []() {
			auto p = std::make_unique<Program>(
				spk::readTextFile("resources/shaders/texture_mesh/vertex.glsl"),
				spk::readTextFile("resources/shaders/texture_mesh/fragment.glsl"));

			p->bindUniformBlock("CameraData", CameraUBORenderCommand::MatrixUBOBindingPoint);
			p->bindUniformBlock("Model3DData", ModelBindingPoint);
			p->bindSampler("uTexture", TextureBindingPoint);
			p->validate();
			return p;
		}();
		return *program;
	}

	UniformBuffer &TextureMesh3DRenderCommand::_sharedModelBuffer()
	{
		static UniformBuffer buffer(ModelBindingPoint, sizeof(Matrix4x4));
		return buffer;
	}

	TextureMesh3DRenderCommand::TextureMesh3DRenderCommand(const Texture *texture, TextureMesh3D mesh, Matrix4x4 model) :
		_texture(texture),
		_mesh(std::move(mesh)),
		_modelMatrix(model),
		_sampler(TextureBindingPoint, Sampler::Filtering::Nearest)
	{
		if (!texture)
		{
			throw spk::Exception("TextureMesh3D renderer texture cannot be null");
		}
		_sampler.setTexture(texture);
		_sampler.validate();
	}

	void TextureMesh3DRenderCommand::execute(RenderContext &context) const
	{
		if (_mesh.empty())
		{
			return;
		}

		auto &model = _sharedModelBuffer();
		model.setData(_modelMatrix);
		model.validate();

		auto &program = _sharedProgram();
		program.activate(context);

		CameraUBORenderCommand::activate(context);

		model.activate(context);
		_sampler.activate(context);
		_mesh.layout().activate(context);
		// ::glEnable(GL_DEPTH_TEST);
		// ::glDepthFunc(GL_LEQUAL);
		// ::glEnable(GL_CULL_FACE);
		// ::glCullFace(GL_CCW);
		program.render(Program::Primitive::Triangles, _mesh.indexType(), 0, _mesh.indexCount());
	}
}
