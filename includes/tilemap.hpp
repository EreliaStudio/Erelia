#pragma once

#include "chunk.hpp"

class Tilemap : public spk::ITilemap<BakableChunk>
	{
	private:
		void _onChunkGeneration(const Tilemap::ChunkCoordinate &p_coordinates, spk::SafePointer<BakableChunk> p_chunk) override
		{
			for (size_t i = 0; i < 16; ++i)
			{
				for (size_t j = 0; j < 16; ++j)
				{
					p_chunk->setContent(i, j, 0, (i == 0 || j == 0 ? 1 : 0));
				}
			}
		}

	public:
		Tilemap()
		{
		}
	};
