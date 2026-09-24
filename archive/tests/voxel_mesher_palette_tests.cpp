#include "voxel_test_utils.hpp"

#include "rendering/command/voxel_render_command.hpp"
#include "voxel/chunk_mesher.hpp"
#include "voxel/material_resolver.hpp"
#include "voxel/palette.hpp"
#include "voxel/voxel_mesher.hpp"
#include "voxel/voxel_model.hpp"

#include <gtest/gtest.h>

#include <algorithm>
#include <set>
#include <iterator>
#include <ranges>
#include <span>
#include <string>
#include <string_view>
#include <utility>

namespace
{
	class SlotResolver final : public voxel::MaterialResolver
	{
	public:
		[[nodiscard]] voxel::Palette::ElementIndex resolve(const Context &context) const override
		{
			if (context.polygon.materialSlot == "top") return 2;
			if (context.polygon.materialSlot == "side") return 1;
			return 0;
		}
	};

	class CoordinateResolver final : public voxel::MaterialResolver
	{
	public:
		[[nodiscard]] voxel::Palette::ElementIndex resolve(const Context &context) const override
		{
			return static_cast<voxel::Palette::ElementIndex>(context.coordinate.x);
		}
	};

	class RecordingResolver final : public voxel::MaterialResolver
	{
	public:
		mutable const voxel::VoxelVolume *volume = nullptr;
		mutable std::set<voxel::Voxel::Side> sides;
		mutable std::size_t calls = 0;

		[[nodiscard]] voxel::Palette::ElementIndex resolve(const Context &context) const override
		{
			volume = &context.volume;
			sides.insert(context.polygon.outerSide);
			++calls;
			EXPECT_EQ(context.coordinate, spk::Vector3Int(0, 0, 0));
			EXPECT_EQ(context.definition.name, "grass");
			EXPECT_EQ(context.cell.orientation(), voxel::Voxel::Orientation::PositiveX);
			return 0;
		}
	};

	class CountingResolver final : public voxel::MaterialResolver
	{
	public:
		mutable std::size_t calls = 0;

		[[nodiscard]] voxel::Palette::ElementIndex resolve(const Context &) const override
		{
			++calls;
			return 0;
		}
	};

	template <typename TType>
	bool byteIdentical(std::span<const TType> left, std::span<const TType> right)
	{
		return std::ranges::equal(std::as_bytes(left), std::as_bytes(right));
	}

	bool byteIdentical(const voxel::VoxelMesh &left, const voxel::VoxelMesh &right)
	{
		return byteIdentical(left.layout().vertexBuffer().cast<voxel::VoxelVertex>(),
				right.layout().vertexBuffer().cast<voxel::VoxelVertex>()) &&
			byteIdentical(left.layout().indexBuffer().cast<voxel::VoxelMesh::Index>(),
				right.layout().indexBuffer().cast<voxel::VoxelMesh::Index>());
	}

	void setCell(voxel::VoxelVolume &volume, const playground_test::VoxelCatalog &catalog,
		spk::Vector3Int coordinate, std::string_view definition)
	{
		volume.edit().set(coordinate, voxel::Voxel::Cell(catalog.id(std::string(definition))));
	}
}

TEST(VoxelMesherPaletteTest, ResolvesOneFlatPaletteElementForEveryEmittedPolygon)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelVolume volume({1, 1, 1}, 1.0f);
	setCell(volume, catalog, {0, 0, 0}, "grass");
	const auto mesh = voxel::VoxelMesher(catalog).bake(volume, SlotResolver{});
	const auto vertices = mesh.layout().vertexBuffer().cast<voxel::VoxelVertex>();
	const auto top = [](const voxel::VoxelVertex &vertex) {
		return vertex.normal == spk::Vector3(0.0f, 1.0f, 0.0f);
	};
	const auto bottom = [](const voxel::VoxelVertex &vertex) {
		return vertex.normal == spk::Vector3(0.0f, -1.0f, 0.0f);
	};

	ASSERT_EQ(mesh.vertexCount(), 24);
	EXPECT_EQ(std::ranges::count_if(vertices, top), 4);
	EXPECT_EQ(std::ranges::count_if(vertices, bottom), 4);
	EXPECT_TRUE(std::ranges::all_of(vertices | std::views::filter(top), [](const auto &vertex) {
		return vertex.paletteElementIndex == 2;
	}));
	EXPECT_TRUE(std::ranges::all_of(vertices | std::views::filter(bottom), [](const auto &vertex) {
		return vertex.paletteElementIndex == 0;
	}));
	EXPECT_EQ(std::ranges::count_if(vertices, [](const auto &vertex) {
		return vertex.paletteElementIndex == 1;
	}), 16);
}

TEST(VoxelMesherPaletteTest, KeepsDifferentPaletteElementsDistinctDuringVertexReuse)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelVolume volume({2, 1, 1}, 1.0f);
	setCell(volume, catalog, {0, 0, 0}, "stone");
	setCell(volume, catalog, {1, 0, 0}, "stone");
	const auto mesh = voxel::VoxelMesher(catalog).bake(volume, CoordinateResolver{});
	const auto vertices = mesh.layout().vertexBuffer().cast<voxel::VoxelVertex>();
	const auto seam = [](const voxel::VoxelVertex &vertex) {
		return vertex.position == spk::Vector3(1.0f, 1.0f, 0.0f) &&
			vertex.normal == spk::Vector3(0.0f, 1.0f, 0.0f);
	};

	ASSERT_EQ(std::ranges::count_if(vertices, seam), 2);
	const auto first = std::ranges::find_if(vertices, seam);
	const auto second = std::ranges::find_if(std::next(first), vertices.end(), seam);
	EXPECT_NE(first->paletteElementIndex, second->paletteElementIndex);
}

TEST(VoxelMesherPaletteTest, ResolverReceivesTheFullTransformedPolygonContext)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelVolume volume({1, 1, 1}, 1.0f);
	volume.edit().set({0, 0, 0}, voxel::Voxel::Cell(
		catalog.id("grass"), voxel::Voxel::Orientation::PositiveX));
	RecordingResolver resolver;
	static_cast<void>(voxel::VoxelMesher(catalog).bake(volume, resolver));

	EXPECT_EQ(resolver.volume, &volume);
	EXPECT_EQ(resolver.calls, 6);
	EXPECT_EQ(resolver.sides, (std::set<voxel::Voxel::Side>{
		voxel::Voxel::Side::NegativeX, voxel::Voxel::Side::PositiveX,
		voxel::Voxel::Side::NegativeY, voxel::Voxel::Side::PositiveY,
		voxel::Voxel::Side::NegativeZ, voxel::Voxel::Side::PositiveZ}));
}

TEST(VoxelMesherPaletteTest, ModelAndChunkUseTheSameDeterministicMeshRepresentation)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelModel model({16, 16, 16}, 1.0f);
	setCell(model, catalog, {0, 0, 0}, "debug_stair");
	voxel::Chunk::Collection chunks;
	auto chunk = playground_test::chunkWithCell(
		{0, 0, 0}, {0, 0, 0}, voxel::Voxel::Cell(catalog.id("debug_stair")));
	voxel::Chunk *source = chunk.get();
	ASSERT_TRUE(playground_test::publish(chunks, std::move(chunk)));
	const SlotResolver resolver;
	voxel::VoxelMesher modelMesher(catalog);
	voxel::Chunk::Mesher chunkMesher(catalog, chunks);
	const auto first = modelMesher.bake(model, resolver);
	const auto repeated = modelMesher.bake(model, resolver);
	const auto chunkMesh = chunkMesher.bake(*source, resolver);

	EXPECT_TRUE(byteIdentical(first, repeated));
	EXPECT_TRUE(byteIdentical(first, chunkMesh));
}

TEST(VoxelMesherPaletteTest, PaletteAndTransformChangesReuseTheSameMeshWithoutRemeshing)
{
	auto catalog = playground_test::loadVoxelCatalog();
	voxel::VoxelModel model({1, 1, 1}, 1.0f);
	setCell(model, catalog, {0, 0, 0}, "stone");
	CountingResolver resolver;
	const auto mesh = voxel::VoxelMesher(catalog).bake(model, resolver);
	const auto vertices = std::vector<voxel::VoxelVertex>(
		mesh.layout().vertexBuffer().cast<voxel::VoxelVertex>().begin(),
		mesh.layout().vertexBuffer().cast<voxel::VoxelVertex>().end());
	const auto indices = std::vector<voxel::VoxelMesh::Index>(
		mesh.layout().indexBuffer().cast<voxel::VoxelMesh::Index>().begin(),
		mesh.layout().indexBuffer().cast<voxel::VoxelMesh::Index>().end());
	voxel::Palette human;
	human.resize(1);
	human.data(0).color = {0.7f, 0.5f, 0.4f, 1.0f};
	human.validate();
	voxel::Palette orc;
	orc.resize(1);
	orc.data(0).color = {0.2f, 0.5f, 0.1f, 1.0f};
	orc.validate();

	voxel::VoxelRenderCommand(human, mesh, spk::Matrix4x4::identity());
	voxel::VoxelRenderCommand(orc, mesh,
		spk::Matrix4x4::translation(spk::Vector3{2.0f, 0.0f, 0.0f}));

	EXPECT_EQ(resolver.calls, 6);
	EXPECT_TRUE(byteIdentical(mesh.layout().vertexBuffer().cast<voxel::VoxelVertex>(),
		std::span<const voxel::VoxelVertex>(vertices)));
	EXPECT_TRUE(byteIdentical(mesh.layout().indexBuffer().cast<voxel::VoxelMesh::Index>(),
		std::span<const voxel::VoxelMesh::Index>(indices)));
}
