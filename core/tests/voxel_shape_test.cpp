#include "erelia/core/voxel/catalog.hpp"

#include "voxel_resource_test_utils.hpp"

#include <gtest/gtest.h>

#include <algorithm>
#include <array>
#include <atomic>
#include <cmath>
#include <cstdint>
#include <thread>
#include <vector>

namespace
{
	constexpr std::int32_t VertexScale = 1000;

	const std::string AsymmetricPolygon = R"({
		"slot":"face",
		"vertices":[
			{"x":0.1,"y":0.2,"z":0.0},
			{"x":0.1,"y":0.8,"z":0.0},
			{"x":0.9999,"y":0.8,"z":0.0},
			{"x":0.8,"y":0.2,"z":0.0}
		]
	})";

	[[nodiscard]] spk::Vector3Int transformed(
		spk::Vector3Int source,
		Voxel::Cell::Orientation orientation,
		Voxel::Cell::FlipOrientation flip)
	{
		spk::Vector3Int result = source;
		switch (orientation)
		{
		case Voxel::Cell::Orientation::PositiveX:
			break;
		case Voxel::Cell::Orientation::NegativeZ:
			result.x = source.z;
			result.z = VertexScale - source.x;
			break;
		case Voxel::Cell::Orientation::NegativeX:
			result.x = VertexScale - source.x;
			result.z = VertexScale - source.z;
			break;
		case Voxel::Cell::Orientation::PositiveZ:
			result.x = VertexScale - source.z;
			result.z = source.x;
			break;
		}
		if (flip == Voxel::Cell::FlipOrientation::NegativeY)
		{
			result.y = VertexScale - source.y;
		}
		return result;
	}

	void expectNormal(const spk::Vector3 &actual, const spk::Vector3 &expected)
	{
		EXPECT_NEAR(actual.x, expected.x, 0.00001f);
		EXPECT_NEAR(actual.y, expected.y, 0.00001f);
		EXPECT_NEAR(actual.z, expected.z, 0.00001f);
	}
}

TEST(VoxelShape, LoadsApprovedCubeSlabSlopeAndStairResources)
{
	Voxel::Catalog catalog;
	catalog.loadShape(voxel_test::shapeResourcePath());

	ASSERT_TRUE(catalog.shapes().contains("cube"));
	ASSERT_TRUE(catalog.shapes().contains("slab"));
	ASSERT_TRUE(catalog.shapes().contains("slope"));
	ASSERT_TRUE(catalog.shapes().contains("stair"));
	EXPECT_FALSE(catalog.shapes().contains("cross"));

	EXPECT_EQ(catalog.shapes().at("cube").polygons().size(), 6u);
	EXPECT_EQ(catalog.shapes().at("slab").polygons().size(), 6u);
	EXPECT_EQ(catalog.shapes().at("slope").polygons().size(), 5u);
	EXPECT_EQ(catalog.shapes().at("stair").polygons().size(), 10u);

	std::int32_t slabMaximumY = 0;
	for (const auto &polygon : catalog.shapes().at("slab").polygons())
	{
		for (const auto &vertex : polygon.vertices)
		{
			slabMaximumY = std::max(slabMaximumY, vertex.y);
		}
	}
	EXPECT_EQ(slabMaximumY, 500);

	const auto &slope = catalog.shapes().at("slope");
	const auto slopeTop = std::ranges::find_if(slope.polygons(), [](const auto &polygon) {
		return polygon.slot == "top";
	});
	ASSERT_NE(slopeTop, slope.polygons().end());
	EXPECT_LT(slopeTop->normal.x, 0.0f);
	EXPECT_GT(slopeTop->normal.y, 0.0f);
	EXPECT_NEAR(slopeTop->normal.z, 0.0f, 0.00001f);

	std::size_t stairTopCount = 0;
	bool hasHalfHeightTread = false;
	bool hasFullHeightTread = false;
	for (const auto &polygon : catalog.shapes().at("stair").polygons())
	{
		if (polygon.slot != "top")
		{
			continue;
		}
		++stairTopCount;
		hasHalfHeightTread |= std::ranges::all_of(
			polygon.vertices,
			[](const auto &vertex) {
				return vertex.y == 500;
			});
		hasFullHeightTread |= std::ranges::all_of(
			polygon.vertices,
			[](const auto &vertex) {
				return vertex.y == 1000;
			});
	}
	EXPECT_EQ(stairTopCount, 2u);
	EXPECT_TRUE(hasHalfHeightTread);
	EXPECT_TRUE(hasFullHeightTread);
}

TEST(VoxelShape, ConvertsNormalizedJsonCoordinatesToDiscreteVertices)
{
	const voxel_test::TemporaryJsonFile file(voxel_test::shapeFile("asymmetric", AsymmetricPolygon));
	Voxel::Catalog catalog;
	catalog.loadShape(file.path());

	const auto &polygon = catalog.shapes().at("asymmetric").polygons().front();
	ASSERT_EQ(polygon.vertices.size(), 4u);
	EXPECT_EQ(polygon.vertices[0], spk::Vector3Int(100, 200, 0));
	EXPECT_EQ(polygon.vertices[1], spk::Vector3Int(100, 800, 0));
	EXPECT_EQ(polygon.vertices[2], spk::Vector3Int(999, 800, 0));
	EXPECT_EQ(polygon.vertices[3], spk::Vector3Int(800, 200, 0));
	EXPECT_EQ(polygon.slot, "face");
	expectNormal(polygon.normal, spk::Vector3(0.0f, 0.0f, -1.0f));
}

TEST(VoxelShape, AppliesAllEightOrientationAndFlipVariantsExactly)
{
	const voxel_test::TemporaryJsonFile file(voxel_test::shapeFile("asymmetric", AsymmetricPolygon));
	Voxel::Catalog catalog;
	catalog.loadShape(file.path());
	const auto &shape = catalog.shapes().at("asymmetric");
	const auto canonical = shape.polygons().front();

	const std::array orientations = {
		Voxel::Cell::Orientation::PositiveX,
		Voxel::Cell::Orientation::NegativeZ,
		Voxel::Cell::Orientation::NegativeX,
		Voxel::Cell::Orientation::PositiveZ};
	const std::array flips = {
		Voxel::Cell::FlipOrientation::PositiveY,
		Voxel::Cell::FlipOrientation::NegativeY};

	for (const auto flip : flips)
	{
		for (const auto orientation : orientations)
		{
			const auto &array = shape.orientedPolygons(orientation, flip);
			ASSERT_FALSE(array.uuid.isNull());
			ASSERT_EQ(array.polygons.size(), 1u);
			const auto &polygon = array.polygons.front();
			EXPECT_EQ(polygon.slot, canonical.slot);
			ASSERT_EQ(polygon.vertices.size(), canonical.vertices.size());

			for (std::size_t index = 0; index < canonical.vertices.size(); ++index)
			{
				const std::size_t sourceIndex =
					flip == Voxel::Cell::FlipOrientation::NegativeY ? canonical.vertices.size() - 1u - index : index;
				EXPECT_EQ(polygon.vertices[index], transformed(canonical.vertices[sourceIndex], orientation, flip));
			}

			const auto first = spk::Vector3(polygon.vertices[1] - polygon.vertices[0]);
			const auto second = spk::Vector3(polygon.vertices[2] - polygon.vertices[0]);
			expectNormal(polygon.normal, first.cross(second).normalized());
		}
	}
}

TEST(VoxelShape, CanonicalVariantIsMaterializedAndLazyVariantKeepsOneUuid)
{
	const voxel_test::TemporaryJsonFile file(voxel_test::shapeFile("asymmetric", AsymmetricPolygon));
	Voxel::Catalog catalog;
	catalog.loadShape(file.path());
	const auto &shape = catalog.shapes().at("asymmetric");

	const auto &canonical = shape.orientedPolygons(
		Voxel::Cell::Orientation::PositiveX,
		Voxel::Cell::FlipOrientation::PositiveY);
	EXPECT_FALSE(canonical.uuid.isNull());
	EXPECT_EQ(&canonical.polygons, &shape.polygons());

	const auto &first = shape.orientedPolygons(
		Voxel::Cell::Orientation::NegativeX,
		Voxel::Cell::FlipOrientation::NegativeY);
	const spk::UUID uuid = first.uuid;
	ASSERT_FALSE(uuid.isNull());

	const auto &second = shape.orientedPolygons(
		Voxel::Cell::Orientation::NegativeX,
		Voxel::Cell::FlipOrientation::NegativeY);
	EXPECT_EQ(&first, &second);
	EXPECT_EQ(second.uuid, uuid);
}

TEST(VoxelShape, ConcurrentFirstAccessPublishesOneImmutableVariant)
{
	const voxel_test::TemporaryJsonFile file(voxel_test::shapeFile("asymmetric", AsymmetricPolygon));
	Voxel::Catalog catalog;
	catalog.loadShape(file.path());
	const auto &shape = catalog.shapes().at("asymmetric");

	constexpr std::size_t ThreadCount = 12;
	std::array<const Voxel::Shape::OrientedPolygonArray *, ThreadCount> arrays{};
	std::array<spk::UUID, ThreadCount> uuids{};
	std::array<std::thread, ThreadCount> threads;
	std::atomic<bool> start = false;

	for (std::size_t index = 0; index < ThreadCount; ++index)
	{
		threads[index] = std::thread([&, index] {
			while (!start.load(std::memory_order_acquire))
			{
				std::this_thread::yield();
			}
			arrays[index] = &shape.orientedPolygons(
				Voxel::Cell::Orientation::PositiveZ,
				Voxel::Cell::FlipOrientation::NegativeY);
			uuids[index] = arrays[index]->uuid;
		});
	}
	start.store(true, std::memory_order_release);
	for (auto &thread : threads)
	{
		thread.join();
	}

	ASSERT_NE(arrays[0], nullptr);
	ASSERT_FALSE(uuids[0].isNull());
	for (std::size_t index = 1; index < ThreadCount; ++index)
	{
		EXPECT_EQ(arrays[index], arrays[0]);
		EXPECT_EQ(uuids[index], uuids[0]);
		ASSERT_EQ(arrays[index]->polygons.size(), arrays[0]->polygons.size());
		EXPECT_EQ(arrays[index]->polygons.front().vertices, arrays[0]->polygons.front().vertices);
	}
}

TEST(VoxelShape, RejectsMalformedPolygonGeometryAndSchema)
{
	const std::array invalidPolygons = {
		R"({"slot":"","vertices":[{"x":0,"y":0,"z":0},{"x":0,"y":1,"z":0},{"x":1,"y":0,"z":0}]})",
		R"({"slot":"face","vertices":[{"x":0,"y":0,"z":0},{"x":0,"y":1,"z":0}]})",
		R"({"slot":"face","vertices":[{"x":0,"y":0,"z":0},{"x":0,"y":1,"z":0},{"x":0,"y":1,"z":0},{"x":1,"y":0,"z":0}]})",
		R"({"slot":"face","vertices":[{"x":0,"y":0,"z":0},{"x":0,"y":1,"z":0},{"x":1,"y":0,"z":0},{"x":0,"y":0,"z":0}]})",
		R"({"slot":"face","vertices":[{"x":0,"y":0,"z":0},{"x":0.5,"y":0.5,"z":0},{"x":1,"y":1,"z":0}]})",
		R"({"slot":"face","vertices":[{"x":0,"y":0,"z":0},{"x":0,"y":1,"z":0},{"x":1,"y":1,"z":0},{"x":1,"y":0,"z":0.1}]})",
		R"({"slot":"face","vertices":[{"x":0,"y":0,"z":0},{"x":0,"y":1,"z":0},{"x":0.5,"y":0.5,"z":0},{"x":1,"y":1,"z":0},{"x":1,"y":0,"z":0}]})",
		R"({"slot":"face","vertices":[{"x":-0.01,"y":0,"z":0},{"x":0,"y":1,"z":0},{"x":1,"y":0,"z":0}]})",
		R"({"slot":"face","vertices":[{"x":0,"y":0,"z":0,"unexpected":1},{"x":0,"y":1,"z":0},{"x":1,"y":0,"z":0}]})"};

	for (std::size_t index = 0; index < invalidPolygons.size(); ++index)
	{
		const voxel_test::TemporaryJsonFile file(
			voxel_test::shapeFile("invalid-" + std::to_string(index), invalidPolygons[index]),
			"invalid-shape");
		Voxel::Catalog catalog;
		EXPECT_THROW(catalog.loadShape(file.path()), spk::Exception) << "fixture index " << index;
		EXPECT_FALSE(catalog.shapes().contains("invalid-" + std::to_string(index)));
	}
}
