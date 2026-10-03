#include "erelia/client/world_manager.hpp"

#include "erelia/client/service.hpp"
#include "erelia/client/widget_order.hpp"

#include <utility>

WorldManager::WorldManager(
	std::string name,
	TerrainStreamingBehaviour::Ranges ranges,
	spk::Widget *parent) :
	spk::EngineWidget(std::move(name), parent),
	_ranges(ranges)
{
	setZOrder(Client::WidgetOrder::World);

	_worldChangedContract =
		Service::clientEventCenter().worldChanged().subscribe(
			[this](World *world) {
				_changeWorld(world);
			});

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

void WorldManager::_changeWorld(World *world)
{
	setEngine(nullptr);
	_world = world;
	_player = nullptr;

	if (_world != nullptr)
	{
		setEngine(&_world->engine());
	}
}

void WorldManager::_instantiatePlayer(
	const PlayerInformation &information)
{
	if (_world == nullptr || _player != nullptr)
	{
		return;
	}

	_player = _world->addEntity<Player>(
		information,
		*_world,
		_ranges);
}

World *WorldManager::world() noexcept
{
	return _world;
}

const World *WorldManager::world() const noexcept
{
	return _world;
}

Player *WorldManager::player() noexcept
{
	return _player;
}

const Player *WorldManager::player() const noexcept
{
	return _player;
}
