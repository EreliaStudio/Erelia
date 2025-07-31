#pragma once

#include <sparkle.hpp>

#include "tilemap.hpp"

class World : public spk::GameObject
{
private:
	Tilemap _tilemap;
	static inline bool _needInitialization = false;
	static inline NodeCollection _nodeCollection;

	static void populateNodeMap();

public:
	World(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent);

	Tilemap& tilemap();
	static NodeCollection& nodeCollection();
};