#include <sparkle>

#include <filesystem>

#include "behaviour/first_person_controller.hpp"
#include "engine/camera.hpp"
#include "engine/camera_holder_3d.hpp"
#include "engine/chunk_render_system.hpp"
#include "voxel/chunk_views.hpp"
#include "voxel/voxel_world.hpp"
#include "ui/chunk_debug_screen.hpp"

int main()
{
	spk::Application application;
	spk::Window &window = application.createWindow("voxel-playground", spk::Window::Configuration{.title = "Sparkle voxel world", .area = spk::Rect2D{spk::Vector2Int{120, 80}, spk::Vector2UInt{1280, 720}}, .backgroundColor = {0.30f, 0.48f, 0.72f, 1.0f}});
	window.root().setGeometry(spk::Rect2D{spk::Vector2Int{0, 0}, spk::Vector2UInt{1280, 720}});

	spk::Engine engine;
	spk::EngineWidget engineWidget("Voxel view", &window.root());
	engineWidget.setGeometry(window.root().geometry());
	engineWidget.setEngine(&engine);
	engineWidget.activate();
	engine.addSystem<playground::ChunkRenderSystem>(window.profiler());
	playground::ChunkDebugScreen debugScreen(&window.root());
	debugScreen.setGeometry(spk::Rect2D{spk::Vector2Int{20, 20}, spk::Vector2UInt{760, 180}});
	debugScreen.activate();

	const auto resourceRoot = std::filesystem::path("resources");
	auto catalog = voxel::Voxel::Catalog<voxel::Voxel::Definition>::load(resourceRoot / "catalog_config.json");

	spk::Camera camera;
	spk::Entity3D cameraEntity("Voxel camera");
	const spk::Vector3 cameraPosition{28.0f, 27.0f, 38.0f};
	cameraEntity.transform().place(cameraPosition);
	cameraEntity.transform().setRotation(spk::Quaternion::lookAt(cameraPosition, {0.0f, 5.0f, 0.0f}));
	camera.configure(spk::Camera::PerspectiveConfiguration{.verticalFieldOfViewRadians = 1.0f, .nearPlane = 0.1f, .farPlane = 500.0f});
	engine.addEntity(&cameraEntity);
	camera.setAsMain(engine);
	cameraEntity.addBehaviour<spk::CameraHolder3D>(&camera);
	cameraEntity.addBehaviour<playground::FirstPersonController>();

	voxel::Chunk::Collection chunks;
	voxel::Chunk::DebugGenerator generator(chunks, catalog, 0xE7E11AULL);
	voxel::Chunk::Mesher mesher(catalog, chunks);
	voxel::Chunk::ViewCollection views(engine, &catalog.atlas(), chunks);
	voxel::Chunk::BakeScheduler scheduler(chunks, mesher, &window.profiler());
	auto bakedChunks = scheduler.subscribeToBakeCompletion([&views](voxel::Chunk::Coordinate coordinate, const spk::TextureMesh3D &mesh) {
		views.setMesh(coordinate, mesh);
	});
	voxel::Chunk::Requester requester(chunks, 1);
	requester.focus({0, 0, 0});
	scheduler.process();

	const int result = application.run();
	engine.removeEntity(&cameraEntity);
	return result;
}
