#pragma once

#include <sparkle.hpp>

#include "ssbo_factory.hpp"

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
	bool isAutotiled;
	Animation animation;
};

class NodeCollection
{
private:
	spk::OpenGL::ShaderStorageBufferObject _nodeCollectionSSBO;
	Node::ID _maxNodeID = 0;
	std::unordered_map<Node::ID, Node> _nodes;

public:
	NodeCollection() :
		_nodeCollectionSSBO(SSBOFactory::nodeCollectionSSBO())
	{

	}

	void updateSSBO()
	{
		_nodeCollectionSSBO.clear();

		_nodeCollectionSSBO.fixedData()[L"nbNode"] = static_cast<int>(_maxNodeID);
		_nodeCollectionSSBO.dynamicArray().clear();
		_nodeCollectionSSBO.dynamicArray().resize(static_cast<int>(_maxNodeID));
		for (Node::ID i = 0; i < _maxNodeID; i++)
		{
			auto& element = _nodeCollectionSSBO.dynamicArray()[i];

			element[L"sprite"] = _nodes[i].sprite;

			auto& animationElement = element[L"animation"];
			animationElement[L"nbFrame"] = _nodes[i].animation.nbFrame;
			animationElement[L"offsetPerFrame"] = _nodes[i].animation.offsetPerFrame;
			animationElement[L"duration"] = _nodes[i].animation.duration;
		} 

		_nodeCollectionSSBO.validate();
	}

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
		return (nullptr);
	}
};