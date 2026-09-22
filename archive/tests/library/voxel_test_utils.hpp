#pragma once

#include "voxel/chunk_collection.hpp"
#include "voxel/chunk_editor.hpp"
#include "voxel/voxel_catalog.hpp"

#include <memory>

namespace playground_test
{
	using VoxelCatalog = voxel::Voxel::Catalog<voxel::Voxel::Definition>;

	[[nodiscard]] VoxelCatalog loadVoxelCatalog();

	[[nodiscard]] std::unique_ptr<voxel::Chunk> chunkWithCell(
		voxel::Chunk::Coordinate coordinate,
		spk::Vector3Int local,
		voxel::Voxel::Cell cell);

	[[nodiscard]] bool publish(voxel::Chunk::Collection &chunks, std::unique_ptr<voxel::Chunk> chunk);
}
