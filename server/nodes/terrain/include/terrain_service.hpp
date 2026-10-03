#pragma once
#include "terrain_event_center.hpp"
#include "terrain_world_collection.hpp"
#include <network/remote_node.hpp>
namespace Service
{
	[[nodiscard]] Terrain::EventCenter &terrainEventCenter();
	[[nodiscard]] spk::RemoteNode::Endpoint &terrainEndpoint();
	[[nodiscard]] TerrainWorldCollection &terrainWorldCollection();
}
