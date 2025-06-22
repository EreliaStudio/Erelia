#include "menu/main_menu.hpp"

void MainMenu::_onGeometryChange()
{
	_layout.setGeometry({0, geometry().size});
}

MainMenu::MainMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::Screen(p_name, p_parent),
	_newGameButton(p_name + L"/NewGameButton", this),
	_loadGameButton(p_name + L"/LoadGameButton", this),
	_exitButton(p_name + L"/ExitButton", this)
{
	_newGameButton.setText(L"New Game");
	_newGameButton.activate();

	_loadGameButton.setText(L"Load Game");
	_loadGameButton.activate();
	
	_exitButton.setText(L"Exit");
	_exitButton.activate();

	_layout.setElementPadding(10);

	_layout.addWidget(&_newGameButton);
	_layout.addWidget(&_loadGameButton);
	_layout.addWidget(&_exitButton);
}

void MainMenu::onNewGameRequest(const spk::PushButton::Job& p_job)
{
	_newGameButton.setOnClick(p_job);
}

void MainMenu::onLoadGameRequest(const spk::PushButton::Job& p_job)
{
	_loadGameButton.setOnClick(p_job);
}

void MainMenu::onExitRequest(const spk::PushButton::Job& p_job)
{
	_exitButton.setOnClick(p_job);
}