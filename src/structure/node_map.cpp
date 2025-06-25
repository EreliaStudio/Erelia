#include "structure/node_map.hpp"

void NodeMap::addNode(const ID& p_id, const Node& p_node)
{
	_nodes[p_id] = p_node;
}

bool NodeMap::contains(const ID& p_id) const
{
	return _nodes.contains(p_id);
}

const Node& NodeMap::node(const ID& p_id) const
{
	auto it = _nodes.find(p_id);
	if (it != _nodes.end())
	{
		return it->second;
	}
	throw std::out_of_range("Node ID not found");
}