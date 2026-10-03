#pragma once

#include "erelia/client/event_center.hpp"
#include "erelia/client/terrain_streaming_behaviour.hpp"
#include "erelia/core/player_information.hpp"
#include "erelia/core/world.hpp"

#include <string>

#include <ui/widget/engine_widget.hpp>

class WorldManager final : public spk::EngineWidget
{
private:
	World *_world = nullptr;
	Player *_player = nullptr;
	TerrainStreamingBehaviour::Ranges _ranges;
	Core::Event<World *>::Contract _worldChangedContract;
	Core::Event<const PlayerInformation &>::Contract _playerReadyContract;

	void _changeWorld(World *world);
	void _instantiatePlayer(const PlayerInformation &information);

public:
	WorldManager(
		std::string name,
		TerrainStreamingBehaviour::Ranges ranges,
		spk::Widget *parent = nullptr);
	~WorldManager() override;

	[[nodiscard]] World *world() noexcept;
	[[nodiscard]] const World *world() const noexcept;
	[[nodiscard]] Player *player() noexcept;
	[[nodiscard]] const Player *player() const noexcept;
};
