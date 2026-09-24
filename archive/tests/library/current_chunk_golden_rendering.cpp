#include "current_chunk_golden_fixture.hpp"
#include "voxel_mesher_fixture.hpp"
#include "voxel_test_utils.hpp"

#include "engine/camera.hpp"
#include "rendering/command/camera_ubo_render_command.hpp"
#include "rendering/command/texture_mesh_3d_render_command.hpp"
#include "voxel/chunk_mesher.hpp"
#include "voxel/chunk_editor.hpp"
#include "voxel/debug_chunk_generator.hpp"
#include "voxel/voxel_mesher.hpp"

#include <GL/glew.h>
#include <sparkle_test.hpp>

#include <algorithm>
#include <iostream>
#include <memory>
#include <exception.hpp>
#include <string>

namespace playground_test::golden
{
	namespace
	{
		constexpr spk::Vector2UInt FramebufferSize{640, 480};
		using Catalog = voxel::Voxel::Catalog<voxel::Voxel::Definition>;

		void require(bool condition, std::string_view message)
		{
			if (!condition) throw spk::Exception(std::string(message));
		}

		Catalog loadCatalog() { return Catalog::load("resources/catalog_config.json"); }

		void appendCell(voxel::Chunk::Collection &chunks, const Catalog &catalog, const Cell &cell)
		{
			voxel::Chunk *chunk = chunks.find(cell.chunk);
			if (chunk == nullptr)
			{
				auto created = std::make_unique<voxel::Chunk>(cell.chunk);
				chunk = created.get();
				require(chunks.request(cell.chunk), "fixture Chunk request failed");
				chunks.publish(std::move(created));
			}
			chunk->edit().set(cell.local, voxel::Voxel::Cell(catalog.id(std::string(cell.definition)), cell.orientation, cell.flip));
		}

		void configureCamera(const Camera &fixture, spk::RenderContext &context)
		{
			spk::Camera camera;
			camera.configure(spk::Camera::PerspectiveConfiguration{
				fixture.verticalFieldOfViewRadians, fixture.nearPlane, fixture.farPlane});
			camera.setViewportSize(FramebufferSize);
			spk::Entity3D entity("Golden camera");
			entity.transform().place(fixture.position);
			entity.transform().setRotation(spk::Quaternion::lookAt(fixture.position, fixture.target));
			spk::CameraUBORenderCommand(camera.projection(), entity.transform().inverseModelMatrix()).execute(context);
		}

		void prepareContext(sparkle_test::OpenGLTestContext &openGL)
		{
			openGL.reset();
			::glViewport(0, 0, static_cast<GLsizei>(FramebufferSize.x), static_cast<GLsizei>(FramebufferSize.y));
			::glClearColor(0.30f, 0.48f, 0.72f, 1.0f);
			::glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);
			require(::glGetError() == GL_NO_ERROR, "failed to prepare deterministic OpenGL state");
		}

		std::vector<voxel::Chunk::Coordinate> sceneCoordinates(const Scene &fixture)
		{
			std::vector<voxel::Chunk::Coordinate> result;
			for (const auto &cell : fixture.cells)
			{
				const auto matchingCoordinate = [&cell](const voxel::Chunk::Coordinate &coordinate) { return coordinate == cell.chunk; };
				if (std::ranges::find_if(result, matchingCoordinate) == result.end()) result.push_back(cell.chunk);
			}
			return result;
		}

		void renderChunks(const Catalog &catalog, const voxel::Chunk::Collection &chunks,
			const std::vector<voxel::Chunk::Coordinate> &coordinates, spk::RenderContext &context, bool alterFirstUv)
		{
			voxel::Chunk::Mesher mesher(catalog, chunks);
			bool alterationPending = alterFirstUv;
			for (const auto coordinate : coordinates)
			{
				const voxel::Chunk *chunk = chunks.find(coordinate);
				require(chunk != nullptr, "render fixture references an absent Chunk");
				auto mesh = mesher.bake(*chunk);
				if (alterationPending)
				{
					spk::TextureMesh3D::Builder altered;
					const auto vertices = mesh.layout().vertexBuffer().cast<spk::Texture3DVertex>();
					const auto indices = mesh.layout().indexBuffer().cast<spk::TextureMesh3D::Index>();
					for (const auto &sourceVertex : vertices)
					{
						auto vertex = sourceVertex;
						vertex.uv = {0.0f, 0.0f};
						static_cast<void>(altered.addVertex(vertex));
					}
					for (const auto index : indices) altered.addIndex(index);
					mesh = std::move(altered).build();
					alterationPending = false;
				}
				const auto origin = voxel::Chunk::worldOrigin(coordinate);
				spk::TextureMesh3DRenderCommand(&catalog.atlas(), mesh,
					spk::Matrix4x4::translation(spk::Vector3(origin))).execute(context);
			}
		}

		Camera rotatedCamera(const Camera &source, std::size_t quarterTurns)
		{
			Camera result = source;
			const float dx = source.position.x - source.target.x;
			const float dz = source.position.z - source.target.z;
			switch (quarterTurns % ViewCount)
			{
			case 1: result.position.x = source.target.x - dz; result.position.z = source.target.z + dx; break;
			case 2: result.position.x = source.target.x - dx; result.position.z = source.target.z - dz; break;
			case 3: result.position.x = source.target.x + dz; result.position.z = source.target.z - dx; break;
			default: break;
			}
			return result;
		}

		void renderVolume(const Catalog &catalog, const voxel::VoxelVolume &volume,
			spk::Vector3 translation, spk::RenderContext &context)
		{
			auto mesh = voxel::VoxelMesher(catalog).bake(volume);
			spk::TextureMesh3DRenderCommand(&catalog.atlas(), std::move(mesh),
				spk::Matrix4x4::translation(translation)).execute(context);
		}

		std::unique_ptr<voxel::Chunk> filledBoundaryChunk(voxel::Chunk::Coordinate coordinate, int firstX)
		{
			auto chunk = std::make_unique<voxel::Chunk>(coordinate);
			auto editor = chunk->edit();
			for (int x = firstX; x < firstX + 2; ++x)
				for (int y = 2; y < 4; ++y)
					for (int z = 4; z < 6; ++z)
						editor.set({x, y, z}, voxel::Voxel::Cell(3));
			return chunk;
		}

		void renderMeshedChunk(const Catalog &catalog, const voxel::Chunk::Collection &chunks,
			const voxel::Chunk &chunk, spk::RenderContext &context)
		{
			auto mesh = voxel::Chunk::Mesher(catalog, chunks).bake(chunk);
			const auto origin = voxel::Chunk::worldOrigin(chunk.coordinate());
			spk::TextureMesh3DRenderCommand(&catalog.atlas(), std::move(mesh),
				spk::Matrix4x4::translation(spk::Vector3(origin))).execute(context);
		}
	}

	void configureTestPaths()
	{
		sparkle_test::configurePaths(PLAYGROUND_TEST_RESOURCE_DIR, PLAYGROUND_TEST_RESULT_DIR);
		auto &openGL = sparkle_test::OpenGLTestContext::instance();
		openGL.makeCurrent();
		std::cout << "OpenGL vendor: " << reinterpret_cast<const char *>(::glGetString(GL_VENDOR)) << '\n'
			<< "OpenGL renderer: " << reinterpret_cast<const char *>(::glGetString(GL_RENDERER)) << '\n'
			<< "OpenGL version: " << reinterpret_cast<const char *>(::glGetString(GL_VERSION)) << '\n';
	}

	void renderScene(const Scene &fixture, std::size_t viewIndex, const std::filesystem::path &path, bool alterFirstUv)
	{
		auto catalog = loadCatalog();
		voxel::Chunk::Collection chunks;
		for (const auto &cell : fixture.cells) appendCell(chunks, catalog, cell);
		auto &openGL = sparkle_test::OpenGLTestContext::instance();
		prepareContext(openGL);
		auto &context = openGL.renderContext();
		configureCamera(fixture.cameras.at(viewIndex), context);
		renderChunks(catalog, chunks, sceneCoordinates(fixture), context, alterFirstUv);
		openGL.save(path);
	}

	void renderSeededScene(std::size_t viewIndex, const std::filesystem::path &path)
	{
		auto catalog = loadCatalog();
		voxel::Chunk::Collection chunks;
		voxel::Chunk::DebugGenerator generator(chunks, catalog, 0xE7E11AULL);
		const std::vector<voxel::Chunk::Coordinate> coordinates{{-1, 0, -1}, {0, 0, -1}, {-1, 0, 0}, {0, 0, 0}};
		for (const auto coordinate : coordinates) require(chunks.request(coordinate), "seeded Chunk request failed");
		auto &openGL = sparkle_test::OpenGLTestContext::instance();
		prepareContext(openGL);
		auto &context = openGL.renderContext();
		configureCamera(seededSceneCameras().at(viewIndex), context);
		renderChunks(catalog, chunks, coordinates, context, false);
		openGL.save(path);
	}

	void renderVoxelMesherModel(bool procedural, std::size_t viewIndex, const std::filesystem::path &path)
	{
		auto catalog = loadCatalog();
		auto model = loadVoxelMesherModel();
		auto volume = makeVoxelMesherVolume();
		auto &openGL = sparkle_test::OpenGLTestContext::instance();
		prepareContext(openGL);
		auto &context = openGL.renderContext();
		const Camera primary{{2.3f, 1.7f, 2.7f}, {0.4f, 0.3f, 0.8f}, 0.72f, 0.05f, 20.0f};
		configureCamera(rotatedCamera(primary, viewIndex), context);
		renderVolume(catalog, procedural ? *volume : *model, {}, context);
		openGL.save(path);
	}

	void renderVoxelMesherPair(const std::filesystem::path &path)
	{
		auto catalog = loadCatalog();
		auto model = loadVoxelMesherModel();
		auto volume = makeVoxelMesherVolume();
		auto &openGL = sparkle_test::OpenGLTestContext::instance();
		prepareContext(openGL);
		auto &context = openGL.renderContext();
		configureCamera({{3.4f, 1.8f, 3.2f}, {1.0f, 0.3f, 0.8f}, 0.72f, 0.05f, 25.0f}, context);
		renderVolume(catalog, *model, {}, context);
		renderVolume(catalog, *volume, {1.2f, 0.0f, 0.0f}, context);
		openGL.save(path);
	}

	void renderChunkMesherOcclusion(bool publishNeighbor, bool renderNeighbor, const std::filesystem::path &path)
	{
		auto catalog = loadCatalog();
		voxel::Chunk::Collection chunks;
		auto left = filledBoundaryChunk({0, 0, 0}, 14);
		voxel::Chunk *leftSource = left.get();
		require(playground_test::publish(chunks, std::move(left)), "left Chunk publication failed");
		if (publishNeighbor)
			require(playground_test::publish(chunks, filledBoundaryChunk({1, 0, 0}, 0)), "right Chunk publication failed");
		auto &openGL = sparkle_test::OpenGLTestContext::instance();
		prepareContext(openGL);
		auto &context = openGL.renderContext();
		configureCamera({{23.0f, 7.0f, 10.0f}, {15.5f, 3.0f, 5.0f}, 0.58f, 0.1f, 50.0f}, context);
		renderMeshedChunk(catalog, chunks, *leftSource, context);
		if (renderNeighbor) renderMeshedChunk(catalog, chunks, *chunks.find({1, 0, 0}), context);
		openGL.save(path);
	}
}
