#include "structure/tilemap.hpp"

void Tilemap::_onChunkGeneration(const ChunkCoordinate& p_coordinates, spk::SafePointer<Chunk> p_chunk)
{
	p_chunk->setPosition(p_coordinates);

	for (size_t i = 0; i < Chunk::Size.x; i++)
	{
		for (size_t j = 0; j < Chunk::Size.y; j++)
		{
			p_chunk->setContent(i, j, 0, (i == 0 || j == 0 ? 1 : 0));
		}
	}

	for (int i = -1; i <= 1; i++)
	{
		for (int j = -1; j <= 1; j++)
		{
			spk::SafePointer<Chunk> tmpChunk = chunk(p_coordinates + ChunkCoordinate(i, j));

			if (tmpChunk != nullptr)
			{
				tmpChunk->unbake();
			}
		}
	}
}

Tilemap::Tilemap() :
	spk::ITilemap<Chunk>()
{
	_loadNodeMap();
}

NodeMap& Tilemap::nodeMap()
{
	return (_nodeMap);
}

const NodeMap& Tilemap::nodeMap() const
{
	return (_nodeMap);
}

void Tilemap::reset()
{
	clear();
	_seed = L"00000000";
}

void Tilemap::setSeed(const std::wstring& p_seed)
{
	_seed = p_seed;
}

void Tilemap::save(const std::filesystem::path& p_rootPath)
{
	spk::JSON::File outputFile = spk::JSON::File();

	outputFile.root().addAttribute(L"Seed") = _seed;
	
	outputFile.save(p_rootPath / fileName);
}

void Tilemap::load(const std::filesystem::path& p_rootPath)
{
	spk::JSON::File inputFile;

	_seed = inputFile[L"Seed"].as<std::wstring>();
}