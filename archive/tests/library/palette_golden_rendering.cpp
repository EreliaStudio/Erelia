#include "current_chunk_golden_fixture.hpp"
#include "voxel_mesher_fixture.hpp"
#include "voxel_test_utils.hpp"

#include "engine/camera.hpp"
#include "rendering/command/camera_ubo_render_command.hpp"
#include "rendering/command/voxel_render_command.hpp"
#include "voxel/chunk_mesher.hpp"
#include "voxel/material_resolver.hpp"
#include "voxel/palette.hpp"
#include "voxel/voxel_mesher.hpp"

#include <GL/glew.h>
#include <sparkle_test.hpp>

#include <initializer_list>
#include <memory>

#include <exception.hpp>

namespace playground_test::golden
{
	namespace
	{
		constexpr spk::Vector2UInt PaletteFramebufferSize{640, 480};

		class DefinitionIndexResolver final : public voxel::MaterialResolver
		{
		public:
			[[nodiscard]] voxel::Palette::ElementIndex resolve(const Context &context) const override
			{
				return static_cast<voxel::Palette::ElementIndex>(context.definition.id - 1);
			}
		};

		voxel::Palette makePalette(std::initializer_list<voxel::Palette::Data> data)
		{
			voxel::Palette palette;
			palette.resize(data.size());
			voxel::Palette::ElementIndex index = 0;
			for (const auto &element : data) palette.data(index++) = element;
			palette.validate();
			return palette;
		}

		voxel::Palette humanPalette()
		{
			return makePalette({{{0.35f, 0.58f, 0.22f, 1.0f}}, {{0.35f, 0.20f, 0.12f, 1.0f}},
				{{0.76f, 0.55f, 0.42f, 1.0f}}, {{0.30f, 0.52f, 0.18f, 1.0f}},
				{{0.42f, 0.30f, 0.20f, 1.0f}}, {{0.64f, 0.50f, 0.30f, 1.0f}},
				{{0.80f, 0.70f, 0.42f, 1.0f}}});
		}

		voxel::Palette orcPalette()
		{
			return makePalette({{{0.20f, 0.42f, 0.12f, 1.0f}}, {{0.24f, 0.15f, 0.08f, 1.0f}},
				{{0.36f, 0.60f, 0.20f, 1.0f}}, {{0.12f, 0.34f, 0.08f, 1.0f}},
				{{0.30f, 0.22f, 0.14f, 1.0f}}, {{0.46f, 0.36f, 0.18f, 1.0f}},
				{{0.58f, 0.48f, 0.24f, 1.0f}}});
		}

		voxel::Palette worldPalette()
		{
			return makePalette({{{0.24f, 0.58f, 0.16f, 1.0f}}, {{0.38f, 0.22f, 0.10f, 1.0f}},
				{{0.50f, 0.52f, 0.56f, 1.0f}}, {{0.16f, 0.48f, 0.12f, 1.0f}},
				{{0.58f, 0.46f, 0.30f, 1.0f}}, {{0.65f, 0.52f, 0.32f, 1.0f}},
				{{0.72f, 0.62f, 0.38f, 1.0f}}});
		}

		void preparePaletteContext(sparkle_test::OpenGLTestContext &openGL)
		{
			openGL.reset();
			::glViewport(0, 0, static_cast<GLsizei>(PaletteFramebufferSize.x),
				static_cast<GLsizei>(PaletteFramebufferSize.y));
			::glClearColor(0.30f, 0.48f, 0.72f, 1.0f);
			::glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);
			if (::glGetError() != GL_NO_ERROR)
				throw spk::Exception("failed to prepare Palette golden OpenGL state");
		}

		void configurePaletteCamera(const Camera &fixture, spk::RenderContext &context)
		{
			spk::Camera camera;
			camera.configure(spk::Camera::PerspectiveConfiguration{
				fixture.verticalFieldOfViewRadians, fixture.nearPlane, fixture.farPlane});
			camera.setViewportSize(PaletteFramebufferSize);
			spk::Entity3D entity("Palette golden camera");
			entity.transform().place(fixture.position);
			entity.transform().setRotation(spk::Quaternion::lookAt(fixture.position, fixture.target));
			spk::CameraUBORenderCommand(camera.projection(), entity.transform().inverseModelMatrix()).execute(context);
		}

		void appendWorldPaletteCells(voxel::Chunk &chunk, const VoxelCatalog &catalog)
		{
			auto editor = chunk.edit();
			editor.set({0, 0, 0}, voxel::Voxel::Cell(catalog.id("grass")));
			editor.set({1, 0, 0}, voxel::Voxel::Cell(catalog.id("dirt")));
			editor.set({2, 0, 0}, voxel::Voxel::Cell(catalog.id("stone")));
			editor.set({0, 1, 2}, voxel::Voxel::Cell(catalog.id("plant")));
			editor.set({2, 1, 2}, voxel::Voxel::Cell(
				catalog.id("plant"), voxel::Voxel::Orientation::PositiveX));
		}
	}

	void renderPaletteModel(bool useOrcPalette, const std::filesystem::path &path)
	{
		auto catalog = loadVoxelCatalog();
		auto model = loadVoxelMesherModel();
		const DefinitionIndexResolver resolver;
		const auto mesh = voxel::VoxelMesher(catalog).bake(*model, resolver);
		const auto palette = useOrcPalette ? orcPalette() : humanPalette();
		auto &openGL = sparkle_test::OpenGLTestContext::instance();
		preparePaletteContext(openGL);
		auto &context = openGL.renderContext();
		configurePaletteCamera({{2.3f, 1.7f, 2.7f}, {0.4f, 0.3f, 0.8f}, 0.72f, 0.05f, 20.0f}, context);
		voxel::VoxelRenderCommand(palette, mesh, spk::Matrix4x4::identity()).execute(context);
		openGL.save(path);
	}

	void renderPaletteModelPair(const std::filesystem::path &path)
	{
		auto catalog = loadVoxelCatalog();
		auto model = loadVoxelMesherModel();
		const DefinitionIndexResolver resolver;
		const auto mesh = voxel::VoxelMesher(catalog).bake(*model, resolver);
		const auto human = humanPalette();
		const auto orc = orcPalette();
		auto &openGL = sparkle_test::OpenGLTestContext::instance();
		preparePaletteContext(openGL);
		auto &context = openGL.renderContext();
		configurePaletteCamera({{3.4f, 1.8f, 3.2f}, {1.0f, 0.3f, 0.8f}, 0.72f, 0.05f, 25.0f}, context);
		voxel::VoxelRenderCommand(human, mesh, spk::Matrix4x4::identity()).execute(context);
		voxel::VoxelRenderCommand(orc, mesh,
			spk::Matrix4x4::translation(spk::Vector3{1.2f, 0.0f, 0.0f})).execute(context);
		openGL.save(path);
	}

	void renderWorldPaletteChunk(const std::filesystem::path &path)
	{
		auto catalog = loadVoxelCatalog();
		voxel::Chunk::Collection chunks;
		auto chunk = std::make_unique<voxel::Chunk>(voxel::Chunk::Coordinate{0, 0, 0});
		appendWorldPaletteCells(*chunk, catalog);
		voxel::Chunk *source = chunk.get();
		if (!publish(chunks, std::move(chunk))) throw spk::Exception("Palette Chunk publication failed");
		const DefinitionIndexResolver resolver;
		const auto mesh = voxel::Chunk::Mesher(catalog, chunks).bake(*source, resolver);
		const auto palette = worldPalette();
		auto &openGL = sparkle_test::OpenGLTestContext::instance();
		preparePaletteContext(openGL);
		auto &context = openGL.renderContext();
		configurePaletteCamera({{10.0f, 8.0f, 12.0f}, {2.5f, 1.0f, 1.5f}, 0.68f, 0.1f, 60.0f}, context);
		voxel::VoxelRenderCommand(palette, mesh, spk::Matrix4x4::identity()).execute(context);
		openGL.save(path);
	}
}
