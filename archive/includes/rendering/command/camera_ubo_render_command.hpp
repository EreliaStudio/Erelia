#pragma once

#include <cstddef>

#include "math/matrix.hpp"
#include "rendering/render_command.hpp"

namespace spk
{
	class UniformBuffer;

	class CameraUBORenderCommand final : public RenderCommand
	{
	public:
		static constexpr std::size_t MatrixUBOBindingPoint = 1;

	private:
		Matrix4x4 _projection;
		Matrix4x4 _view;

		[[nodiscard]] static UniformBuffer &_sharedBuffer();

	public:
		explicit CameraUBORenderCommand(const Matrix4x4 &projection, const Matrix4x4 &view);

		static void activate(RenderContext &renderContext);
		void execute(RenderContext &renderContext) const override;
	};
}
