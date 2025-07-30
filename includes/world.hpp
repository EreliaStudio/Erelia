#pragma once

#include <sparkle.hpp>

#include "tilemap.hpp"

class World : public spk::GameObject
{
private:
	Tilemap _tilemap;

public:
	World(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent);
};