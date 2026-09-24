#include "voxel/debug_chunk_generator.hpp"

namespace voxel
{
	Chunk::DebugGenerator::DebugGenerator(Collection &collection, const Voxel::Catalog<Voxel::Definition> &catalog, std::uint64_t seed) :
		Generator(collection),
		_catalog(catalog),
		_seed(seed),
		_grass(catalog.id("grass")),
		_dirt(catalog.id("dirt")),
		_stone(catalog.id("stone")),
		_plant(catalog.id("plant")),
		_slab(catalog.id("debug_slab")),
		_slope(catalog.id("debug_slope")),
		_stair(catalog.id("debug_stair"))
	{
	}

	std::unique_ptr<Chunk> Chunk::DebugGenerator::generate(Coordinate coordinate) const
	{
		auto chunk = std::make_unique<Chunk>(coordinate);
		auto editor = chunk->edit();
		const auto origin = Chunk::worldOrigin(coordinate);
		auto hash = [this](int x, int z) {
			std::uint64_t value = _seed ^ (static_cast<std::uint64_t>(static_cast<std::uint32_t>(x)) << 32) ^ static_cast<std::uint32_t>(z);
			value ^= value >> 33;
			value *= 0xff51afd7ed558ccdULL;
			value ^= value >> 33;
			return value;
		};

		for (int z = 0; z < Chunk::Size; ++z)
		{
			for (int x = 0; x < Chunk::Size; ++x)
			{
				const int worldX = origin.x + x;
				const int worldZ = origin.z + z;
				const int height = 5 + static_cast<int>(hash(worldX / 3, worldZ / 3) % 7);
				for (int y = 0; y < Chunk::Size; ++y)
				{
					const int worldY = origin.y + y;
					if (worldY > height)
					{
						continue;
					}
					const Voxel::ID id = worldY == height ? _grass : (worldY >= height - 3 ? _dirt : _stone);
					editor.set({x, y, z}, Voxel::Cell(id));
				}
				if (height + 1 >= origin.y && height + 1 < origin.y + Chunk::Size && hash(worldX, worldZ) % 43 == 0)
				{
					editor.set({x, height + 1 - origin.y, z}, Voxel::Cell(_plant, static_cast<Voxel::Orientation>((hash(worldX, worldZ) >> 8) & Voxel::Cell::OrientationMask)));
				}
				if (worldZ == 0 && worldX >= -3 && worldX <= 4 && height + 1 >= origin.y && height + 1 < origin.y + Chunk::Size)
				{
					const int sample = worldX + 3;
					const Voxel::ID shape = sample < 2 ? _slab : (sample < 6 ? _slope : _stair);
					const auto orientation = static_cast<Voxel::Orientation>((sample % 4) << 29);
					const auto flip = (sample % 2) == 0 ? Voxel::Flip::PositiveY : Voxel::Flip::NegativeY;
					editor.set({x, height + 1 - origin.y, z}, Voxel::Cell(shape, orientation, flip));
				}
			}
		}
		return chunk;
	}
}
