#include "erelia/server/terrain/prototype_chunk_provider.hpp"

#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/voxel/cell.hpp"

#include <array>
#include <cstdint>
#include <utility>

namespace
{
	struct FixturePlacement
	{
		Voxel::Volume::LocalCoordinate coordinate;
		Voxel::Cell::Orientation orientation;
		Voxel::Cell::FlipOrientation flip;
	};

	constexpr std::array GroundPlacements = {
		FixturePlacement{{4, 1, 4}, Voxel::Cell::Orientation::PositiveX, Voxel::Cell::FlipOrientation::PositiveY},
		FixturePlacement{{5, 1, 4}, Voxel::Cell::Orientation::NegativeZ, Voxel::Cell::FlipOrientation::PositiveY},
		FixturePlacement{{6, 1, 4}, Voxel::Cell::Orientation::NegativeX, Voxel::Cell::FlipOrientation::PositiveY},
		FixturePlacement{{7, 1, 4}, Voxel::Cell::Orientation::PositiveZ, Voxel::Cell::FlipOrientation::PositiveY},
		FixturePlacement{{4, 1, 5}, Voxel::Cell::Orientation::PositiveX, Voxel::Cell::FlipOrientation::NegativeY},
		FixturePlacement{{5, 1, 5}, Voxel::Cell::Orientation::NegativeZ, Voxel::Cell::FlipOrientation::NegativeY},
		FixturePlacement{{6, 1, 5}, Voxel::Cell::Orientation::NegativeX, Voxel::Cell::FlipOrientation::NegativeY},
		FixturePlacement{{7, 1, 5}, Voxel::Cell::Orientation::PositiveZ, Voxel::Cell::FlipOrientation::NegativeY}};

	constexpr std::array ElevatedPlacements = {
		FixturePlacement{{4, 4, 10}, Voxel::Cell::Orientation::PositiveX, Voxel::Cell::FlipOrientation::PositiveY},
		FixturePlacement{{5, 4, 10}, Voxel::Cell::Orientation::NegativeZ, Voxel::Cell::FlipOrientation::PositiveY},
		FixturePlacement{{6, 4, 10}, Voxel::Cell::Orientation::NegativeX, Voxel::Cell::FlipOrientation::PositiveY},
		FixturePlacement{{7, 4, 10}, Voxel::Cell::Orientation::PositiveZ, Voxel::Cell::FlipOrientation::PositiveY},
		FixturePlacement{{4, 5, 10}, Voxel::Cell::Orientation::PositiveX, Voxel::Cell::FlipOrientation::NegativeY},
		FixturePlacement{{5, 5, 10}, Voxel::Cell::Orientation::NegativeZ, Voxel::Cell::FlipOrientation::NegativeY},
		FixturePlacement{{6, 5, 10}, Voxel::Cell::Orientation::NegativeX, Voxel::Cell::FlipOrientation::NegativeY},
		FixturePlacement{{7, 5, 10}, Voxel::Cell::Orientation::PositiveZ, Voxel::Cell::FlipOrientation::NegativeY}};

	const Voxel::Cell CubeCell(
		1u,
		Voxel::Cell::Orientation::PositiveX,
		Voxel::Cell::FlipOrientation::PositiveY);

	void populateBaselineAndWalls(
		Chunk::Builder &builder,
		const Chunk::Coordinate &coordinate)
	{
		if (coordinate.y != 0)
		{
			return;
		}

		for (std::int32_t z = 0; z < Chunk::Extent; ++z)
		{
			for (std::int32_t x = 0; x < Chunk::Extent; ++x)
			{
				(void)builder.set({x, 0, z}, CubeCell);
			}
		}

		if (coordinate.x == 0)
		{
			for (std::int32_t z = 0; z < Chunk::Extent; ++z)
			{
				for (std::int32_t y = 1; y <= 3; ++y)
				{
					(void)builder.set({0, y, z}, CubeCell);
				}
			}
		}

		if (coordinate.z == 0)
		{
			for (std::int32_t x = 0; x < Chunk::Extent; ++x)
			{
				for (std::int32_t y = 1; y <= 3; ++y)
				{
					(void)builder.set({x, y, 0}, CubeCell);
				}
			}
		}
	}

	void populateShapeFixture(
		Chunk::Builder &builder,
		Voxel::Definition::ID definitionID)
	{
		for (const FixturePlacement &placement : GroundPlacements)
		{
			(void)builder.set(
				placement.coordinate,
				Voxel::Cell(definitionID, placement.orientation, placement.flip));
		}

		for (const FixturePlacement &placement : ElevatedPlacements)
		{
			(void)builder.set(
				placement.coordinate,
				Voxel::Cell(definitionID, placement.orientation, placement.flip));
		}
	}

	void populateDedicatedFixture(
		Chunk::Builder &builder,
		const Chunk::Coordinate &coordinate)
	{
		if (coordinate == Chunk::Coordinate{1, 0, 1})
		{
			populateShapeFixture(builder, 2u);
		}
		else if (coordinate == Chunk::Coordinate{2, 0, 1})
		{
			populateShapeFixture(builder, 3u);
		}
		else if (coordinate == Chunk::Coordinate{1, 0, 2})
		{
			populateShapeFixture(builder, 4u);
		}
	}
}

namespace erelia::server
{
	Chunk PrototypeChunkProvider::provide(
		const Chunk::Coordinate &coordinate)
	{
		Chunk::Builder builder;
		populateBaselineAndWalls(builder, coordinate);
		populateDedicatedFixture(builder, coordinate);
		return std::move(builder).build();
	}
}
