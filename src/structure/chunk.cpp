#include "structure/chunk.hpp"

#include "structure/context.hpp"

namespace
{
    spk::Vector3Int quarterNeighbour[4][3] =
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

	spk::Vector3 cornerPosition[4] =
    {
        {0.0f,0.5f,0.0f},  // NE
        {0.0f,0.0f,0.0f},  // SE
        {0.5f,0.0f,0.0f},  // SW
        {0.5f,0.5f,0.0f}   // NW
    };
}

Chunk::Chunk() : spk::IChunk<NodeMap::ID, 16, 16, 3>()
{
	for (size_t i = 0; i < Size.x; i++)
	{
		for (size_t j = 0; j < Size.y; j++)
		{
			for (size_t h = 0; h < Size.z; h++)
			{
				setContent(i, j, h, -1);
			}	
		}
	}
}

void Chunk::setPosition(const spk::Vector2Int& p_position)
{
	_position = p_position;
}

const spk::Vector2Int& Chunk::position()
{
	return (_position);
}

bool Chunk::isBaked() const
{
	return (_isBaked);
}

void Chunk::_computeNeighbours()
{
	for (int x = -1; x <= 1; x++)
	{
		for (int y = -1; y <= 1; y++)
		{
			_neighbours[x + 1][y + 1] = Context::instance()->tilemap.chunk(_position + spk::Vector2Int{x, y});
		}	
	}
}

spk::SafePointer<Chunk> Chunk::_getNeighbourChunk(const spk::Vector2Int& p_offset) const
{
	return (_neighbours[p_offset.x + 1][p_offset.y + 1]);
}

spk::Vector2Int Chunk::_getNeighbourOffset(const spk::Vector3Int& p_position) const
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

spk::Vector2Int Chunk::_computeSpriteOffset(NodeMap::ID baseIndex, int corner, const spk::Vector3Int& relativePosition) const
{
    bool same[3] {false,false,false};

    for (int side = 0; side < 3; side++) 
    {
        spk::Vector3Int p = relativePosition + quarterNeighbour[corner][side];

		spk::Vector2Int neighbourOffset = _getNeighbourOffset(p);
        spk::SafePointer<Chunk> ch = _getNeighbourChunk(neighbourOffset);
		p -= spk::Vector3Int(neighbourOffset * static_cast<spk::Vector2Int>(Chunk::Size.xy()), 0);
        
		if (ch && ch->content(p) == baseIndex)
		{
            same[side] = true;
		}
    }
    return spriteOffsets[corner][same[0]][same[1]][same[2]];
}

void Chunk::_insertMonotile(const Node& node, const spk::Vector3Int& pos)
{
    _renderer.prepare(
			spk::Vector3(pos) + spk::Vector3(_position, 0) * Size,
			{1.f,1.f},
			node.sprite,
			node.animationOffset,
			node.frameDuration,
			node.nbFrame
		);
}

void Chunk::_insertAutotile(const Node& node, const spk::Vector3Int& pos, NodeMap::ID baseIndex)
{
    for (int corner = 0; corner < 4; corner++)
    {
        spk::Vector2Int subOffset = _computeSpriteOffset(baseIndex, corner, pos);

        _renderer.prepare(
				spk::Vector3(pos) + cornerPosition[corner] + spk::Vector3(_position, 0) * Size,
				{0.5f,0.5f},
				node.sprite + subOffset,
				node.animationOffset,
				node.frameDuration,
				node.nbFrame
			);
    }
}

void Chunk::unbake()
{
	_isBaked = false;
}

void Chunk::bake()
{
	if (_isBaked == true)
	{
		return ;
	}

	_renderer.clear();

	_computeNeighbours();

	for (int x = 0; x < Size.x; x++)
	{
		for (int y = 0; y < Size.y; y++)
		{
			for (int z = 0; z < Size.z; z++)
			{
				Chunk::ContentType index = content(x, y, z);

				if (Context::instance()->tilemap.nodeMap().contains(index) == true)
				{
					const Node& tmpNode = Context::instance()->tilemap.nodeMap().node(index);

					if (tmpNode.isAutotiled == Node::Type::Autotiled)
					{
						_insertAutotile(tmpNode, {x, y, z}, index);
					}
					else
					{
						_insertMonotile(tmpNode, {x, y, z});
					}
				}
			}	
		}	
	}

	_renderer.validate();

	_isBaked = true;
}

void Chunk::render()
{
	if (_isBaked == false)
	{
		bake();	
	}
	
	_renderer.render();
}