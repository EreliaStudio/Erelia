#pragma once

#include <sparkle.hpp>

#include "utils/widget_override.hpp"

class LoadGameMenu : public spk::Screen
{
private:
	PushButton _cancelButton;

public:
	LoadGameMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	void onCancelRequest(const spk::PushButton::Job& p_job);
};