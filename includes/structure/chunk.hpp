#pragma once

#include <sparkle.hpp>

#include <unordered_map>
#include <set>
#include <cstdint>

#include "structure/node_map.hpp"

#include "renderer/chunk_renderer.hpp"

class Chunk : public spk::IChunk<NodeMap::ID, 16, 16, 3>
{
private:
	spk::Vector2Int _position;
	bool _isBaked = false;
	ChunkRenderer _renderer;
    spk::SafePointer<Chunk> _neighbours[3][3] = {
			{nullptr,nullptr,nullptr},
        	{nullptr,nullptr,nullptr},
        	{nullptr,nullptr,nullptr}
		};

	void _insertAutotile (const Node& p_node, const spk::Vector3Int& p_nodePosition, NodeMap::ID baseIndex);
    void _insertMonotile(const Node& p_node, const spk::Vector3Int& p_nodePosition);

    void _computeNeighbours();
	spk::SafePointer<Chunk> _getNeighbourChunk(const spk::Vector2Int& p_offset) const;
	spk::Vector2Int _getNeighbourOffset(const spk::Vector3Int& p_position) const;

    spk::Vector2Int _computeSpriteOffset(NodeMap::ID baseIndex, int quadrant, const spk::Vector3Int& relPos) const;

public:
	Chunk();

	void setPosition(const spk::Vector2Int& p_position);
	const spk::Vector2Int& position();

	bool isBaked() const;
	void unbake();
	void bake();
	void render();
};