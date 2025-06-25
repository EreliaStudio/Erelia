#pragma once

#include <sparkle.hpp>

#include "structure/chunk.hpp"
#include "structure/node_map.hpp"

class Tilemap : public spk::ITilemap<Chunk>
{
private:
	static inline std::wstring fileName = L"world.json";
	std::wstring _seed;

	NodeMap _nodeMap;

	void _onChunkGeneration(const ChunkCoordinate& p_coordinates, spk::SafePointer<Chunk> p_chunk);

	void _loadNodeMap();
public:
	Tilemap();

	NodeMap& nodeMap();
	const NodeMap& nodeMap() const;

	void reset();

	void setSeed(const std::wstring& p_seed);

	void save(const std::filesystem::path& p_rootPath);
	void load(const std::filesystem::path& p_rootPath);
};