#pragma once
#include "terrain_event_center.hpp"
#include "erelia/core/world_collection.hpp"
#include <network/remote_node.hpp>
namespace Service
{
	[[nodiscard]] Terrain::EventCenter &terrainEventCenter();
	[[nodiscard]] spk::RemoteNode::Endpoint &terrainEndpoint();
	[[nodiscard]] WorldCollection &terrainWorldCollection();
}
