#include "menu/game_menu.hpp"

#include "structure/context.hpp"

void GameMenu::_onGeometryChange()
{
	_tilemapManager.setGeometry(0, geometry().size);
	_actorManager.setGeometry(0, geometry().size);
	_playerManager.setGeometry(0, geometry().size);
}

GameMenu::GameMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::Screen(p_name, p_parent),
	_tilemapManager(p_name + L"/WorldManager", this),
	_actorManager(p_name + L"/ActorManager", this),
	_playerManager(p_name + L"/PlayerManager", this)
{
	_tilemapManager.activate();
	_actorManager.activate();
	_playerManager.activate();
}

void GameMenu::loadContext(const std::wstring& p_name)
{
	Context::instance()->clear();

	Context::instance()->load(p_name);
	
	requireGeometryUpdate();
}

void GameMenu::initialize()
{
	_tilemapManager.initialize();
	_actorManager.initialize();
	_playerManager.initialize();
}