#pragma once

#include "engine/behaviour.hpp"

#include <string>

namespace spk
{
	class Camera;
	class Entity3D;

	class CameraHolder3D final : public Behaviour
	{
	private:
		Camera *_camera = nullptr;

		void _onGeometryChange(const Rect2D &geometry) override;
		void _buildRenderSnapshot(RenderSnapshot::Builder &builder) override;

	public:
		CameraHolder3D(
			Camera *camera = nullptr,
			const std::string &name = "Camera holder 3D",
			Entity3D *owner = nullptr);

		void attach(Entity *owner) override;

		[[nodiscard]] Camera *camera() noexcept;
		[[nodiscard]] const Camera *camera() const noexcept;

		void setCamera(Camera *camera);
	};
}
