#pragma once

#include <cstdint>

#include "voxel/chunk_generator.hpp"
#include "voxel/voxel_catalog.hpp"

namespace voxel
{
	class Chunk::DebugGenerator final : public Generator
	{
	private:
		const Voxel::Catalog<Voxel::Definition> &_catalog;
		std::uint64_t _seed;
		Voxel::ID _grass;
		Voxel::ID _dirt;
		Voxel::ID _stone;
		Voxel::ID _plant;
		Voxel::ID _slab;
		Voxel::ID _slope;
		Voxel::ID _stair;

	protected:
		std::unique_ptr<Chunk> generate(Coordinate coordinate) const override;

	public:
		DebugGenerator(Collection &collection, const Voxel::Catalog<Voxel::Definition> &catalog, std::uint64_t seed);
	};
}
