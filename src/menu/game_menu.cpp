#include "menu/game_menu.hpp"

#include "structure/game_file.hpp"

void GameMenu::_initializeGame()
{
	spk::cout << "Initializing game: " << GameFile::instance()->name << std::endl;

	GameFile::instance()->load(GameFile::instance()->name);

	requireGeometryUpdate();
}

GameMenu::GameMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::Screen(p_name, p_parent),
	_tilemapRenderer(p_name + L"/TilemapRenderer", this),
	_actorRenderer(p_name + L"/ActorRenderer", this)
{
	_onActivateContract = addActivationCallback([this](){
		_initializeGame();
	});
}