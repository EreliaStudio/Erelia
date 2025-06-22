#pragma once

#include <sparkle.hpp>

class ActorRenderer : public spk::Widget
{
private:

public:
	ActorRenderer(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);
};