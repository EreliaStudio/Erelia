#include "voxel/chunk_mesher.hpp"

#include <exception.hpp>

namespace voxel
{
	Chunk::Mesher::Mesher(
		const Voxel::Catalog<Voxel::Definition> &catalog,
		const Collection &chunks) :
		VoxelMesher(catalog),
		_chunks(chunks)
	{
	}

	spk::TextureMesh3D Chunk::Mesher::bake(const Chunk &chunk) const
	{
		return VoxelMesher::bake(chunk);
	}

	VoxelMesh Chunk::Mesher::bake(
		const Chunk &chunk, const MaterialResolver &materialResolver) const
	{
		return VoxelMesher::bake(chunk, materialResolver);
	}

	Voxel::Cell Chunk::Mesher::_outsideCell(const VoxelVolume &volume, spk::Vector3Int coordinate) const
	{
		const auto *chunk = dynamic_cast<const Chunk *>(&volume);
		if (chunk == nullptr)
			throw spk::Exception("Chunk::Mesher can only mesh a Chunk");
		const spk::Vector3Int world = Chunk::worldOrigin(chunk->coordinate()) + coordinate;
		return _chunks.worldCell(world).value_or(Voxel::Cell{});
	}
}
