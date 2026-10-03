#include "erelia/client/world_manager.hpp"

#include "erelia/client/service.hpp"
#include "erelia/client/widget_order.hpp"

#include <utility>

WorldManager::WorldManager(
	std::string name,
	TerrainCollections &terrainCollections,
	TerrainStreamingBehaviour::Ranges ranges,
	spk::Widget *parent) :
	spk::EngineWidget(std::move(name), parent),
	_terrainCollections(terrainCollections),
	_ranges(ranges)
{
	setEngine(&_engine);
	setZOrder(Client::WidgetOrder::World);

	_playerReadyContract =
		Service::clientEventCenter().playerReady().subscribe(
			[this](const PlayerInformation &information) {
				_instantiatePlayer(information);
			});

	activate();
}

WorldManager::~WorldManager()
{
	setEngine(nullptr);
}

void WorldManager::_instantiatePlayer(const PlayerInformation &information)
{
	(void)information;

	if (_player != nullptr)
	{
		return;
	}

	_player = std::make_unique<Player>(
		_terrainCollections,
		_ranges);
	_engine.addEntity(_player.get());
}

spk::Engine &WorldManager::gameEngine() noexcept
{
	return _engine;
}

const spk::Engine &WorldManager::gameEngine() const noexcept
{
	return _engine;
}

Player *WorldManager::player() noexcept
{
	return _player.get();
}

const Player *WorldManager::player() const noexcept
{
	return _player.get();
}
