#include "voxel_test_utils.hpp"

namespace playground_test
{
	VoxelCatalog loadVoxelCatalog()
	{
		return VoxelCatalog::load("resources/catalog_config.json");
	}

	std::unique_ptr<voxel::Chunk> chunkWithCell(
		voxel::Chunk::Coordinate coordinate,
		spk::Vector3Int local,
		voxel::Voxel::Cell cell)
	{
		auto chunk = std::make_unique<voxel::Chunk>(coordinate);
		chunk->edit().set(local, cell);
		return chunk;
	}

	bool publish(voxel::Chunk::Collection &chunks, std::unique_ptr<voxel::Chunk> chunk)
	{
		const auto coordinate = chunk->coordinate();
		if (!chunks.request(coordinate)) return false;
		chunks.publish(std::move(chunk));
		return true;
	}
}
