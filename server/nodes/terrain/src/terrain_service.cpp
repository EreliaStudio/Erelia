#include "terrain_service.hpp"

#include "generating_world_provider.hpp"
spk::RemoteNode::Endpoint &Service::terrainEndpoint()
{
	static spk::RemoteNode::Endpoint endpoint;
	return endpoint;
}

WorldService &Service::terrainWorldService()
{
	static WorldService worlds(
		GeneratingWorldProvider{});
	return worlds;
}
