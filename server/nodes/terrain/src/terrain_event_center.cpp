#include "terrain_service.hpp"

Terrain::EventCenter &Service::terrainEventCenter()
{
	static Terrain::EventCenter events;
	return events;
}
