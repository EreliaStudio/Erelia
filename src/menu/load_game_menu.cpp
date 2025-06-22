#include "menu/load_game_menu.hpp"

LoadGameMenu::LoadGameMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::Screen(p_name, p_parent),
	_cancelButton(p_name + L"/Cancel", this)
{
	_cancelButton.setText(L"Cancel");
	_cancelButton.activate();
}

void LoadGameMenu::onCancelRequest(const spk::PushButton::Job& p_job)
{
	_cancelButton.setOnClick(p_job);
}