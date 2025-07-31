#include "chunk.hpp"

#include "context.hpp"

namespace
{
    spk::Vector3Int quarterNeighbours[4][3] =
    {
        { {-1,0,0}, {-1,1,0}, {0,1,0} },     // NE
        { {-1,0,0}, {-1,-1,0},{0,-1,0} },    // SE
        { {0,-1,0}, {1,-1,0}, {1,0,0} },     // SW
        { {1,0,0},  {1,1,0},  {0,1,0} }      // NW
    };

	/*
		AB | DE
		C     F
		-     -
		G     L
		HI | JK
	*/
	spk::Vector2Int spriteOffsets[4][2][2][2] =
	{
		{
			{
				{
					spk::Vector2Int(0, 0),
					spk::Vector2Int(0, 3)
				},
				{
					spk::Vector2Int(0, 2),
					spk::Vector2Int(0, 3)
				}
			},
		 	{
				{
					spk::Vector2Int(1, 2),
					spk::Vector2Int(2, 0)
				},
				{
					spk::Vector2Int(1, 2),
					spk::Vector2Int(1, 3)
				}
			}
		},
		{
			{
				{
					spk::Vector2Int(0, 1),
		   			spk::Vector2Int(0, 4)
				},
		  		{
					spk::Vector2Int(0, 5),
		   			spk::Vector2Int(0, 4)
				}
			},
			{
				{
					spk::Vector2Int(1, 5),
					spk::Vector2Int(2, 1)
				},
				{
					spk::Vector2Int(1, 5),
					spk::Vector2Int(1, 4)
				}
			}
		},
		{
			{
				{
					spk::Vector2Int(1, 1),
					spk::Vector2Int(2, 5)
				},
				{
					spk::Vector2Int(3, 5),
					spk::Vector2Int(2, 5)
				}
			},
			{
				{
					spk::Vector2Int(3, 4),
					spk::Vector2Int(3, 1)
				},
				{
					spk::Vector2Int(3, 4),
					spk::Vector2Int(2, 4)
				}
			}
		},
		{
			{
				{
					spk::Vector2Int(1, 0),
					spk::Vector2Int(3, 3)},
				{
					spk::Vector2Int(3, 2),
					spk::Vector2Int(3, 3)
				}
			},
			{
				{
					spk::Vector2Int(2, 2),
					spk::Vector2Int(3, 0)
				},
				{
					spk::Vector2Int(2, 2),
					spk::Vector2Int(2, 3)
				}
			}
		}
	};

	spk::Vector3 cornerPositions[4] =
    {
        {0.0f,0.5f,0.0f},  // NE
        {0.0f,0.0f,0.0f},  // SE
        {0.5f,0.0f,0.0f},  // SW
        {0.5f,0.5f,0.0f}   // NW
    };
}

void BakableChunk::Renderer::clear()
{
	_bufferSet.clear();
	_transformUBO.clear();
}

void BakableChunk::Renderer::_insertData(spk::Vector3 p_anchor, spk::Vector2 p_size, const spk::SpriteSheet::Sprite& p_sprite, const Node::ID& p_nodeID)
{
	struct Vertex
	{
		spk::Vector3 position;
		spk::Vector2 uvs;
		int nodeIndex;
	};

	size_t numberOfVertices = _bufferSet.layout().size() / sizeof(Vertex);

	float x1 = p_anchor.x;
	float y1 = p_anchor.y;
	float x2 = p_anchor.x + p_size.x;
	float y2 = p_anchor.y + p_size.y;
	float z = p_anchor.z;

	float u1 = p_sprite.anchor.x;
	float v1 = p_sprite.anchor.y + p_sprite.size.y;
	float u2 = p_sprite.anchor.x + p_sprite.size.x;
	float v2 = p_sprite.anchor.y;

	_bufferSet.layout() << Vertex{{x1, y2, z}, {u1, v1}, p_nodeID};
	_bufferSet.layout() << Vertex{{x2, y2, z}, {u2, v1}, p_nodeID};
	_bufferSet.layout() << Vertex{{x1, y1, z}, {u1, v2}, p_nodeID};
	_bufferSet.layout() << Vertex{{x2, y1, z}, {u2, v2}, p_nodeID};

	std::array<unsigned int, 6> indices = {0, 1, 2, 2, 1, 3};
	for (const auto &index : indices)
	{
		_bufferSet.indexes() << index + numberOfVertices;
	}
}

spk::Vector2Int BakableChunk::Renderer::_computeCornerOffset(size_t i, size_t j, size_t k, size_t corner, const Node::ID& nodeID, const PrepareData &p_prepareData)
{
	bool same[3] {false,false,false};

    for (int side = 0; side < 3; side++) 
    {
        spk::Vector3Int p = spk::Vector3Int(i, j, k) + quarterNeighbours[corner][side];

		spk::Vector2Int neighbourOffset = p_prepareData.neighbourOffset(p);
        spk::SafePointer<Chunk> ch = p_prepareData.neighbourChunk(neighbourOffset);
		p -= spk::Vector3Int(neighbourOffset * static_cast<spk::Vector2Int>(Chunk::Size.xy()), 0);
        
		if (ch && ch->content(p) == nodeID)
		{
            same[side] = true;
		}
    }
    return spriteOffsets[corner][same[0]][same[1]][same[2]];
}

void BakableChunk::Renderer::_prepareAutotile(size_t i, size_t j, size_t k, const Node::ID& nodeID, const Node* node, const PrepareData &p_prepareData)
{
	for (size_t corner = 0; corner < 4; corner++)
	{
		spk::Vector2Int spriteOffset = _computeCornerOffset(i, j, k, corner, nodeID, p_prepareData);

		_insertData(
			spk::Vector3(i, j, k) + cornerPositions[corner],
			spk::Vector2(0.5f, 0.5f),
			_tileset->sprite(node->sprite + spriteOffset),
			nodeID
		);
	}
}

void BakableChunk::Renderer::_prepareMonotile(size_t i, size_t j, size_t k, const Node::ID& nodeID, const Node* node, const PrepareData &p_prepareData)
{
	_insertData(
		spk::Vector3(i, j, k),
		spk::Vector2(1.0f, 1.0f),
		_tileset->sprite(node->sprite),
		nodeID
	);
}

void BakableChunk::Renderer::PrepareData::setup(const spk::SafePointer<BakableChunk>& p_chunk)
{
	chunk = p_chunk;

	Tilemap& tilemap = Context::instance()->world.tilemap();
	spk::Vector2Int baseChunkCoord = chunk->coordinates();

	for (int i = -1; i <= 1; i++)
	{
		for (int j = -1; j <= 1; j++)
		{
			neightbours[i + 1][j + 1] = tilemap.chunk(baseChunkCoord + spk::Vector2Int(i, j));
		}
	}
}

spk::SafePointer<Chunk> BakableChunk::Renderer::PrepareData::neighbourChunk(const spk::Vector2Int& p_offset) const
{
	return (neightbours[p_offset.x + 1][p_offset.y + 1]);
}

spk::Vector2Int BakableChunk::Renderer::PrepareData::neighbourOffset(const spk::Vector3Int& p_position) const
{
	spk::Vector2Int result = {0, 0}; 
	if (p_position.x < 0)
	{
		result.x--;
	}
	else if (p_position.x >= Chunk::Size.x)
	{
		result.x++;
	}

	if (p_position.y < 0)
	{
		result.y--;
	}
	else if (p_position.y >= Chunk::Size.y)
	{
		result.y++;
	}

	return (result);
}

void BakableChunk::Renderer::prepare(const spk::Transform& p_transform, const spk::SafePointer<BakableChunk> &p_chunk)
{
	PrepareData prepareData;
	prepareData.setup(p_chunk);

	for (size_t i = 0; i < BakableChunk::Size.x; i++)
	{
		for (size_t j = 0; j < BakableChunk::Size.y; j++)
		{
			for (size_t k = 0; k < BakableChunk::Size.z; k++)
			{
				Node::ID nodeID = p_chunk->content(i, j, k);

				if (nodeID == -1)
				{
					continue;
				}

				const Node* node = Context::instance()->world.nodeCollection().node(nodeID);
			
				if (node == nullptr)
				{
					continue;
				}

				if (node->isAutotiled == true)
				{
					_prepareAutotile(i, j, k, nodeID, node, prepareData);
				}
				else
				{
					_prepareMonotile(i, j, k, nodeID, node, prepareData);
				}
			}
		}
	}
}

void BakableChunk::Renderer::validate()
{
	_bufferSet.validate();
	_transformUBO.validate();
}