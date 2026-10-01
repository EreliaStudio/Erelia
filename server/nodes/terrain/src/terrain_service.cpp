#include "terrain_service.hpp"
spk::RemoteNode::Endpoint &Service::terrainEndpoint()
{
	static spk::RemoteNode::Endpoint endpoint;
	return endpoint;
}
