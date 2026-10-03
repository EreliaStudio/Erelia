#pragma once

#include "erelia/client/event_center.hpp"
#include "erelia/client/terrain_collections.hpp"
#include "erelia/client/terrain_streaming_behaviour.hpp"
#include "erelia/core/player_information.hpp"

#include <engine/engine.hpp>
#include <ui/widget/engine_widget.hpp>

#include <memory>
#include <string>

class WorldManager final : public spk::EngineWidget
{
private:
	spk::Engine _engine;
	TerrainCollections &_terrainCollections;
	TerrainStreamingBehaviour::Ranges _ranges;
	std::unique_ptr<Player> _player;
	Core::Event<const PlayerInformation &>::Contract _playerReadyContract;

	void _instantiatePlayer(const PlayerInformation &information);

public:
	WorldManager(
		std::string name,
		TerrainCollections &terrainCollections,
		TerrainStreamingBehaviour::Ranges ranges,
		spk::Widget *parent = nullptr);
	~WorldManager() override;

	[[nodiscard]] spk::Engine &gameEngine() noexcept;
	[[nodiscard]] const spk::Engine &gameEngine() const noexcept;
	[[nodiscard]] Player *player() noexcept;
	[[nodiscard]] const Player *player() const noexcept;
};
