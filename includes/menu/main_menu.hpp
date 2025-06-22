#pragma once

#include <sparkle.hpp>

#include "utils/widget_override.hpp"

class MainMenu : public spk::Screen
{
private:
	spk::VerticalLayout _layout;
	PushButton _newGameButton;
	PushButton _loadGameButton;
	PushButton _exitButton;

	void _onGeometryChange();

public:
	MainMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	void onNewGameRequest(const spk::PushButton::Job& p_job);
	void onLoadGameRequest(const spk::PushButton::Job& p_job);
	void onExitRequest(const spk::PushButton::Job& p_job);
};