#pragma once

#include <sparkle.hpp>

#include "widget/tilemap_renderer.hpp"
#include "widget/actor_renderer.hpp"

class GameMenu : public spk::Screen
{
private:
	spk::ContractProvider::Contract _onActivateContract;

	TilemapRenderer _tilemapRenderer;
	ActorRenderer _actorRenderer;

	void _initializeGame();

public:
	GameMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);
};