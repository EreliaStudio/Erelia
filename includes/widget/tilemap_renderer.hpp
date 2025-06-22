#pragma once

#include <sparkle.hpp>

class TilemapRenderer : public spk::Widget
{
private:

public:
	TilemapRenderer(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);
};