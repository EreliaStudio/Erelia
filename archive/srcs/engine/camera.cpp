#include "engine/camera.hpp"

#include "engine/engine.hpp"

#include <unordered_map>

#include <exception.hpp>

namespace spk
{
	namespace
	{
		std::unordered_map<Engine *, Camera *> g_mainCameras;
	}

	Camera::Camera()
	{
		configure(PerspectiveConfiguration{});
		_onContextEditionContract = subscribeToContextEdition(
			[this](Engine *const &oldContext, Engine *const &) {
				const auto oldMain = g_mainCameras.find(oldContext);
				if (oldMain != g_mainCameras.end() && oldMain->second == this)
				{
					g_mainCameras.erase(oldMain);
				}
			});
	}

	Camera::~Camera()
	{
		const auto main = g_mainCameras.find(context());
		if (main != g_mainCameras.end() && main->second == this)
		{
			g_mainCameras.erase(main);
		}
	}

	void Camera::setAsMain(Engine &engine)
	{
		changeContext(&engine);
		g_mainCameras[&engine] = this;
	}

	bool Camera::isMain() const
	{
		return context() != nullptr && mainCamera(context()) == this;
	}

	Matrix4x4 Camera::_computeProjection(
		const PerspectiveConfiguration &configuration,
		const Vector2UInt &viewportSize)
	{
		const float aspectRatio = static_cast<float>(viewportSize.x) / static_cast<float>(viewportSize.y);
		return Matrix4x4::perspective(
			configuration.verticalFieldOfViewRadians,
			aspectRatio,
			configuration.nearPlane,
			configuration.farPlane);
	}

	Matrix4x4 Camera::_computeProjection(
		const OrthographicConfiguration &configuration,
		const Vector2UInt &viewportSize)
	{
		if (configuration.verticalSize <= 0.0f || configuration.farPlane <= configuration.nearPlane)
		{
			throw spk::Exception("Invalid orthographic camera configuration");
		}

		const float aspectRatio = static_cast<float>(viewportSize.x) / static_cast<float>(viewportSize.y);
		const float halfHeight = configuration.verticalSize * 0.5f;
		const float halfWidth = halfHeight * aspectRatio;
		return Matrix4x4::ortho(
			-halfWidth,
			halfWidth,
			-halfHeight,
			halfHeight,
			configuration.nearPlane,
			configuration.farPlane);
	}

	Matrix4x4 Camera::_computeProjection(const Configuration &configuration, const Vector2UInt &viewportSize)
	{
		if (viewportSize.x == 0 || viewportSize.y == 0)
		{
			throw spk::Exception("Camera viewport dimensions must be non-zero");
		}

		return std::visit(
			[&viewportSize](const auto &value) {
				return _computeProjection(value, viewportSize);
			},
			configuration);
	}

	void Camera::configure(const PerspectiveConfiguration &configuration)
	{
		Matrix4x4 projection = _computeProjection(configuration, _viewportSize);
		_configuration = configuration;
		_projection = projection;
	}

	void Camera::configure(const OrthographicConfiguration &configuration)
	{
		Matrix4x4 projection = _computeProjection(configuration, _viewportSize);
		_configuration = configuration;
		_projection = projection;
	}

	void Camera::setViewportSize(const Vector2UInt &viewportSize)
	{
		Matrix4x4 projection = _computeProjection(_configuration, viewportSize);
		_viewportSize = viewportSize;
		_projection = projection;
	}

	const Vector2UInt &Camera::viewportSize() const noexcept
	{
		return _viewportSize;
	}

	Matrix4x4 Camera::projection() const noexcept
	{
		return _projection;
	}

	Camera *Camera::mainCamera(Engine *engine)
	{
		const auto main = g_mainCameras.find(engine);
		return main == g_mainCameras.end() ? nullptr : main->second;
	}

	const Camera *Camera::mainCamera(const Engine *engine)
	{
		return mainCamera(const_cast<Engine *>(engine));
	}
}
