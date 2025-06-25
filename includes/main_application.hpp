#pragma once

#include <sparkle.hpp>

#include "menu/game_menu.hpp"
#include "menu/main_menu.hpp"
#include "menu/new_game_menu.hpp"
#include "menu/load_game_menu.hpp"

#include "structure/context.hpp"

class MainApplication : public spk::Widget
{
private:
	Context::Instanciator _gameFileInstanciator;

	MainMenu _mainMenu;
	NewGameMenu _newGameMenu;
	LoadGameMenu _loadGameMenu;
	GameMenu _gameMenu;

	std::function<void()> _onQuitApplication;

	void _onGeometryChange();

	void _createNewGame(const std::wstring& p_name, const std::wstring& p_seed, const spk::Vector2UInt& p_iconSprite);

public:
	MainApplication(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	void setOnQuitApplication(const std::function<void()>& p_callback);
};