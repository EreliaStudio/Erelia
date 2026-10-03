#include "terrain_world_collection.hpp"

#include "terrain_world.hpp"

std::unique_ptr<World> TerrainWorldCollection::_createWorld(
	const Identifier &)
{
	return std::make_unique<TerrainWorld>();
}
