#pragma once

#include <sparkle.hpp>
#include "widget/graphical_widget.hpp"

class ActorManager : public GraphicalWidget
{
private:

public:
	ActorManager(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	void initialize();
};