#include "erelia/client/client_runtime.hpp"

#include "erelia/client/requesting_world_provider.hpp"
#include "erelia/client/service.hpp"
#include "erelia/client/widget_order.hpp"
#include "erelia/core/player_information.hpp"

#include <utility>

ClientRuntime::ClientRuntime(
	ConnectionManager::Endpoint endpoint,
	spk::Timer::Duration retryDelay,
	spk::Widget *parent) :
	_connectionManager(
		"/ClientRuntime/ConnectionManager",
		std::move(endpoint),
		retryDelay,
		parent),
	_networkManager(parent)
{
	_connectionManager.setZOrder(Client::WidgetOrder::Connection);
	_networkManager.setZOrder(Client::WidgetOrder::Network);

	_worldChangedContract =
		Service::clientEventCenter().worldChanged().subscribe(
			[this](World *world) {
				_world = world;
			});

	_connectedContract =
		Service::clientEventCenter().clientConnected().subscribe([] {
			Service::clientEventCenter().playerLoadingRequested().trigger();
		});

	// ST-001-11 has no player-information protocol yet. Keep the lifecycle
	// boundary explicit while preserving the prototype Player bootstrap.
	_playerLoadingRequestedContract =
		Service::clientEventCenter().playerLoadingRequested().subscribe([] {
			const PlayerInformation information;
			Service::clientEventCenter().playerReady().trigger(information);
		});

	static_cast<RequestingWorldProvider &>(
		Service::clientWorldCollection().provider())
		.bind(_networkManager);
}

ClientRuntime::~ClientRuntime()
{
	Service::clientWorldCollection().clear();
	static_cast<RequestingWorldProvider &>(
		Service::clientWorldCollection().provider())
		.unbind(_networkManager);
}

ConnectionManager &ClientRuntime::connectionManager() noexcept
{
	return _connectionManager;
}

ClientNetworkManager &ClientRuntime::networkManager() noexcept
{
	return _networkManager;
}

World *ClientRuntime::world() noexcept
{
	return _world;
}

const World *ClientRuntime::world() const noexcept
{
	return _world;
}
