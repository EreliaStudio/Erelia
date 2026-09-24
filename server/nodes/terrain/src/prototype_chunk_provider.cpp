#include "prototype_chunk_provider.hpp"

#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/voxel/cell.hpp"

#include <array>
#include <cstdint>
#include <functional>
#include <utility>

#include <design_pattern/singleton.hpp>
#include <threading/worker_pool.hpp>

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

	[[nodiscard]] Chunk generateChunk(
		const Chunk::Coordinate &coordinate)
	{
		Chunk::Builder builder;
		populateBaselineAndWalls(builder, coordinate);
		populateDedicatedFixture(builder, coordinate);
		return std::move(builder).build();
	}
}

std::size_t PrototypeChunkProvider::RequestHash::operator()(
	const Chunk::Collection::Request &request) const noexcept
{
	const std::size_t coordinateHash =
		std::hash<Chunk::Coordinate>{}(request.coordinate);
	const std::size_t generationHash =
		std::hash<Chunk::Collection::Generation>{}(request.generation);
	return coordinateHash ^
		   (generationHash + 0x9e3779b9u + (coordinateHash << 6u) +
			(coordinateHash >> 2u));
}

void PrototypeChunkProvider::request(
	const Chunk::Collection::Request &request)
{
	(void)_requested.publish(request);
}

void PrototypeChunkProvider::update(
	Chunk::Collection &collection)
{
	spk::WorkerPool &workerPool =
		spk::Singleton<spk::WorkerPool>::instance();

	RequestSet::container_type requests;
	(void)_requested.drain(requests);

	for (const Chunk::Collection::Request &request : requests)
	{
		if (!collection.isPending(request))
		{
			continue;
		}

		spk::Task<Chunk> task(
			[coordinate = request.coordinate] {
				return generateChunk(coordinate);
			});
		auto answer = workerPool.submit(std::move(task));
		_pending.push_back(PendingTask{request, std::move(answer)});
	}

	auto iterator = _pending.begin();
	while (iterator != _pending.end())
	{
		const spk::Task<Chunk>::Status status =
			iterator->answer.status();

		if (status == spk::Task<Chunk>::Status::Pending)
		{
			++iterator;
			continue;
		}

		if (status == spk::Task<Chunk>::Status::Completed)
		{
			(void)collection.publish(
				iterator->request,
				iterator->answer.result());
		}
		else
		{
			(void)collection.fail(iterator->request);
		}

		iterator = _pending.erase(iterator);
	}
}
