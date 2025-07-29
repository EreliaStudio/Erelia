#pragma once

#include <sparkle.hpp>

class World : public spk::GameObject
{
private:

public:
	World(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent);
};