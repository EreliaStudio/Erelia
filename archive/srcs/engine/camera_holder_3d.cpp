#include "engine/camera_holder_3d.hpp"

#include "engine/camera.hpp"
#include "engine/engine.hpp"
#include "engine/entity3d.hpp"
#include "engine/transform3d.hpp"
#include "rendering/command/camera_ubo_render_command.hpp"

#include <exception.hpp>

namespace spk
{
	CameraHolder3D::CameraHolder3D(Camera *camera, const std::string &name, Entity3D *owner) :
		Behaviour(name),
		_camera(camera)
	{
		attach(owner);
	}

	void CameraHolder3D::attach(Entity *owner)
	{
		if (owner != nullptr && dynamic_cast<Entity3D *>(owner) == nullptr)
		{
			throw spk::Exception("CameraHolder3D can only be attached to an Entity3D");
		}

		Behaviour::attach(owner);
	}

	void CameraHolder3D::setCamera(Camera *camera)
	{
		_camera = camera;
		const Vector2UInt size = geometry().size;
		if (_camera != nullptr && size.x != 0 && size.y != 0)
		{
			_camera->setViewportSize(size);
		}
	}

	Camera *CameraHolder3D::camera() noexcept
	{
		return _camera;
	}

	const Camera *CameraHolder3D::camera() const noexcept
	{
		return _camera;
	}

	void CameraHolder3D::_onGeometryChange(const Rect2D &geometry)
	{
		if (_camera != nullptr && geometry.size.x != 0 && geometry.size.y != 0)
		{
			_camera->setViewportSize(geometry.size);
		}
	}

	void CameraHolder3D::_buildRenderSnapshot(RenderSnapshot::Builder &builder)
	{
		Engine *engine = context();
		auto *entity = dynamic_cast<Entity3D *>(owner());

		if (_camera == nullptr || engine == nullptr || entity == nullptr ||
			Camera::mainCamera(engine) != _camera)
		{
			return;
		}

		builder.renderPass(Engine::PreSceneRenderPassKey)
			.emplace<CameraUBORenderCommand>(
				_camera->projection(),
				entity->transform().inverseModelMatrix());
	}
}
