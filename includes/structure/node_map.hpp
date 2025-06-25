#pragma once

#include <sparkle.hpp>

#include "structure/node.hpp"

class NodeMap
{
public:
	using ID = short;

private:
	std::unordered_map<ID, Node> _nodes;

public:
	NodeMap() = default;

	void addNode(const ID& p_id, const Node& p_node);

	bool contains(const ID& p_id) const;

	const Node& node(const ID& p_id) const;
};