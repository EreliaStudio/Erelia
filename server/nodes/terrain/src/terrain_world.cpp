#include "terrain_world.hpp"

#include "prototype_chunk_provider.hpp"

#include <utility>

TerrainWorld::TerrainWorld(WorldIdentifier identifier) :
	World(std::move(identifier))
{
}

std::unique_ptr<World::Chunks> TerrainWorld::_createChunkCollection()
{
	return std::make_unique<Chunks>(
		PrototypeChunkProvider{});
}

std::unique_ptr<World::Columns> TerrainWorld::_createColumnCollection()
{
	return std::make_unique<Columns>(
		PrototypeColumnProvider{});
}
