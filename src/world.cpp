#include "world.hpp"

World::World(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent) :
	spk::GameObject(p_name, p_parent)
{
	if (_needInitialization == false)
	{
		populateNodeMap();
		_needInitialization = true;
	}
}

Tilemap& World::tilemap()
{
	return (_tilemap);
}

NodeCollection& World::nodeCollection()
{
	return (_nodeCollection);
}

void World::populateNodeMap()
{
	
	_nodeCollection.updateSSBO();
}