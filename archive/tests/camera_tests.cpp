#include <sparkle>
#include <gtest/gtest.h>

#include "engine/camera.hpp"

#include <cmath>
#include <exception.hpp>

namespace
{
	bool approximately(float value, float expected)
	{
		return std::abs(value - expected) < 0.0001f;
	}
}

TEST(CameraTest, UpdatesPerspectiveProjectionForViewportAspectRatio)
{
	spk::Camera camera;
	camera.configure(spk::Camera::PerspectiveConfiguration{.verticalFieldOfViewRadians = 1.0f, .nearPlane = 0.1f, .farPlane = 500.0f});
	camera.setViewportSize({1600, 900});

	const spk::Matrix4x4 perspective = camera.projection();
	const float aspectRatio = 1600.0f / 900.0f;
	EXPECT_TRUE(approximately(perspective[0][0], perspective[1][1] / aspectRatio));

	camera.setViewportSize({800, 800});
	const spk::Matrix4x4 squarePerspective = camera.projection();
	EXPECT_TRUE(approximately(squarePerspective[0][0], squarePerspective[1][1]));
}

TEST(CameraTest, UpdatesOrthographicProjectionForViewportAspectRatio)
{
	spk::Camera camera;
	camera.configure(spk::Camera::OrthographicConfiguration{.verticalSize = 20.0f, .nearPlane = 0.1f, .farPlane = 500.0f});
	camera.setViewportSize({1600, 800});
	const spk::Matrix4x4 orthographic = camera.projection();
	EXPECT_TRUE(approximately(orthographic[0][0], 0.05f));
	EXPECT_TRUE(approximately(orthographic[1][1], 0.1f));
}

TEST(CameraTest, RejectsEmptyViewportWithoutChangingPreviousState)
{
	spk::Camera camera;
	camera.setViewportSize({1600, 800});
	EXPECT_THROW(camera.setViewportSize({1600, 0}), spk::Exception);
	EXPECT_EQ(camera.viewportSize(), spk::Vector2UInt(1600, 800));
}

TEST(CameraTest, ReplacesMainCameraForEngine)
{
	spk::Engine engine;
	spk::Camera camera;
	spk::Camera replacement;
	camera.setAsMain(engine);
	EXPECT_TRUE(camera.isMain());
	EXPECT_EQ(spk::Camera::mainCamera(&engine), &camera);
	replacement.setAsMain(engine);
	EXPECT_FALSE(camera.isMain());
	EXPECT_TRUE(replacement.isMain());
	EXPECT_EQ(spk::Camera::mainCamera(&engine), &replacement);
}
