#include "rendering/command/camera_ubo_render_command.hpp"

#include "graphics/opengl/uniform_buffer.hpp"

namespace spk
{
	UniformBuffer &CameraUBORenderCommand::_sharedBuffer()
	{
		static UniformBuffer buffer(MatrixUBOBindingPoint, sizeof(Matrix4x4) * 2);
		return buffer;
	}

	CameraUBORenderCommand::CameraUBORenderCommand(const Matrix4x4 &projection, const Matrix4x4 &view) :
		_projection(projection),
		_view(view)
	{
	}

	void CameraUBORenderCommand::activate(RenderContext &renderContext)
	{
		_sharedBuffer().activate(renderContext);
	}

	void CameraUBORenderCommand::execute(RenderContext &renderContext) const
	{
		UniformBuffer &buffer = _sharedBuffer();
		auto &castedBuffer = buffer.cast<spk::Matrix4x4[2]>();
		castedBuffer[0] = _view;
		castedBuffer[1] = _projection;

		buffer.validate();
		buffer.activate(renderContext);
	}
}
