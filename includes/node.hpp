#pragma once

#include <sparkle.hpp>

struct Node
{
	using ID = int;

	struct Animation
	{
		int nbFrame;
		spk::Vector2Int offsetPerFrame;
		int duration;
	};

	spk::Vector2Int sprite;
	Animation animation;
};

class NodeCollection
{
private:

	std::unordered_map<Node::ID, Node> _nodes;

public:
	NodeCollection() = default;

	void addNode(const Node::ID &p_id, const Node &p_node)
	{
		_nodes[p_id] = p_node;
	}

	const Node* node(const Node::ID &p_id) const
	{
		if (_nodes.contains(p_id))
		{
			return &_nodes.at(p_id);
		}
	}
};