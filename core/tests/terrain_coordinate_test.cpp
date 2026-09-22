#include "erelia/core/terrain/coordinate.hpp"

#include <gtest/gtest.h>

#include <array>
#include <cstddef>
#include <cstdint>
#include <limits>

namespace
{
	struct ConversionFixture
	{
		spk::Vector3Int global;
		spk::Vector3Int chunk;
		spk::Vector3Int local;
	};

	struct ScalarFixture
	{
		std::int32_t global;
		std::int32_t chunk;
		std::int32_t local;
	};

	constexpr std::array<ConversionFixture, 6> conversionFixtures = {{
		{{0, 0, 0}, {0, 0, 0}, {0, 0, 0}},
		{{15, 15, 15}, {0, 0, 0}, {15, 15, 15}},
		{{16, 16, 16}, {1, 1, 1}, {0, 0, 0}},
		{{-1, -1, -1}, {-1, -1, -1}, {15, 15, 15}},
		{{-16, -16, -16}, {-1, -1, -1}, {0, 0, 0}},
		{{-17, -17, -17}, {-2, -2, -2}, {15, 15, 15}},
	}};

	constexpr std::array<ScalarFixture, 9> scalarFixtures = {{
		{-17, -2, 15},
		{-16, -1, 0},
		{-15, -1, 1},
		{-1, -1, 15},
		{0, 0, 0},
		{1, 0, 1},
		{15, 0, 15},
		{16, 1, 0},
		{17, 1, 1},
	}};

	[[nodiscard]] spk::Vector3Int onAxis(std::size_t axis, std::int32_t value)
	{
		switch (axis)
		{
		case 0:
			return {value, 0, 0};
		case 1:
			return {0, value, 0};
		default:
			return {0, 0, value};
		}
	}

	void expectLocalRangeAndReconstruction(
		const spk::Vector3Int &global,
		const spk::Vector3Int &chunk,
		const spk::Vector3Int &local)
	{
		const std::array<std::int32_t, 3> globalComponents = {global.x, global.y, global.z};
		const std::array<std::int32_t, 3> chunkComponents = {chunk.x, chunk.y, chunk.z};
		const std::array<std::int32_t, 3> localComponents = {local.x, local.y, local.z};

		for (std::size_t axis = 0; axis < globalComponents.size(); ++axis)
		{
			EXPECT_GE(localComponents[axis], 0);
			EXPECT_LT(localComponents[axis], core::terrain::chunkExtent);

			const auto reconstructed =
				static_cast<std::int64_t>(chunkComponents[axis]) * core::terrain::chunkExtent +
				localComponents[axis];
			EXPECT_EQ(reconstructed, globalComponents[axis]);
		}
	}
}

TEST(TerrainCoordinate, ExposesFixedTerrainScale)
{
	EXPECT_EQ(core::terrain::chunkExtent, 16);
	EXPECT_FLOAT_EQ(core::terrain::cellWorldExtent, 1.0F);
}

TEST(TerrainCoordinate, ConvertsExactThreeDimensionalFixtures)
{
	for (const auto &fixture : conversionFixtures)
	{
		SCOPED_TRACE(::testing::Message() << "global=" << fixture.global);

		const auto chunk = core::terrain::toChunkCoordinate(fixture.global);
		const auto local = core::terrain::toLocalCoordinate(fixture.global);

		EXPECT_EQ(chunk, fixture.chunk);
		EXPECT_EQ(local, fixture.local);
		EXPECT_EQ(core::terrain::toChunkCoordinate(fixture.global), chunk);
		EXPECT_EQ(core::terrain::toLocalCoordinate(fixture.global), local);
		expectLocalRangeAndReconstruction(fixture.global, chunk, local);
	}
}

TEST(TerrainCoordinate, ConvertsEachAxisIndependentlyAcrossBoundaries)
{
	for (std::size_t axis = 0; axis < 3; ++axis)
	{
		for (const auto &fixture : scalarFixtures)
		{
			SCOPED_TRACE(
				::testing::Message() << "axis=" << axis << ", global=" << fixture.global);

			const auto global = onAxis(axis, fixture.global);
			const auto expectedChunk = onAxis(axis, fixture.chunk);
			const auto expectedLocal = onAxis(axis, fixture.local);
			const auto chunk = core::terrain::toChunkCoordinate(global);
			const auto local = core::terrain::toLocalCoordinate(global);

			EXPECT_EQ(chunk, expectedChunk);
			EXPECT_EQ(local, expectedLocal);
			expectLocalRangeAndReconstruction(global, chunk, local);
		}
	}
}

TEST(TerrainCoordinate, ConvertsMixedSignsIndependently)
{
	constexpr spk::Vector3Int global{-17, 16, -1};
	constexpr spk::Vector3Int expectedChunk{-2, 1, -1};
	constexpr spk::Vector3Int expectedLocal{15, 0, 15};

	const auto chunk = core::terrain::toChunkCoordinate(global);
	const auto local = core::terrain::toLocalCoordinate(global);

	EXPECT_EQ(chunk, expectedChunk);
	EXPECT_EQ(local, expectedLocal);
	expectLocalRangeAndReconstruction(global, chunk, local);
}

TEST(TerrainCoordinate, HandlesRepresentableIntegerExtremesWithoutOverflow)
{
	constexpr auto minimum = std::numeric_limits<std::int32_t>::min();
	constexpr auto maximum = std::numeric_limits<std::int32_t>::max();
	constexpr spk::Vector3Int global{minimum, maximum, minimum + 1};
	constexpr spk::Vector3Int expectedChunk{-134217728, 134217727, -134217728};
	constexpr spk::Vector3Int expectedLocal{0, 15, 1};

	const auto chunk = core::terrain::toChunkCoordinate(global);
	const auto local = core::terrain::toLocalCoordinate(global);

	EXPECT_EQ(chunk, expectedChunk);
	EXPECT_EQ(local, expectedLocal);
	expectLocalRangeAndReconstruction(global, chunk, local);
}
