#pragma once

#include <sparkle.hpp>

#include "widget/world_manager.hpp"
#include "widget/actor_manager.hpp"
#include "widget/player_manager.hpp"

class GameMenu : public spk::Screen
{
private:
	spk::ContractProvider::Contract _onActivateContract;

	WorldManager _tilemapManager;
	ActorManager _actorManager;
	PlayerManager _playerManager;

	void _onGeometryChange() override;

public:
	GameMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	void loadContext(const std::wstring& p_name);

	void initialize();
};