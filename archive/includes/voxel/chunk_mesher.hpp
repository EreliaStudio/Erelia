#pragma once

#include "voxel/chunk_collection.hpp"
#include "voxel/voxel_mesher.hpp"

namespace voxel
{
	class Chunk::Mesher final : public VoxelMesher
	{
	private:
		const Collection &_chunks;

		[[nodiscard]] Voxel::Cell _outsideCell(
			const VoxelVolume &volume, spk::Vector3Int coordinate) const override;

	public:
		Mesher(const Voxel::Catalog<Voxel::Definition> &catalog, const Collection &chunks);

		[[nodiscard]] spk::TextureMesh3D bake(const Chunk &chunk) const;
		[[nodiscard]] VoxelMesh bake(
			const Chunk &chunk, const MaterialResolver &materialResolver) const;
	};
}
