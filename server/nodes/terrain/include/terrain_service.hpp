#pragma once
#include "terrain_event_center.hpp"
#include <network/remote_node.hpp>
namespace Service
{
	[[nodiscard]] Terrain::EventCenter &terrainEventCenter();
	[[nodiscard]] spk::RemoteNode::Endpoint &terrainEndpoint();
}
