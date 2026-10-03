#pragma once

#include "erelia/client/client_network_manager.hpp"
#include "erelia/client/connection_manager.hpp"
#include "erelia/client/event_center.hpp"
#include "erelia/client/terrain_collections.hpp"

#include <ui/widget.hpp>

class ClientRuntime final
{
private:
	ConnectionManager _connectionManager;
	ClientNetworkManager _networkManager;
	TerrainCollections _terrainCollections;
	Core::Event<>::Contract _connectedContract;
	Core::Event<>::Contract _playerLoadingRequestedContract;

public:
	ClientRuntime(
		ConnectionManager::Endpoint endpoint,
		spk::Timer::Duration retryDelay,
		spk::Widget *parent);

	[[nodiscard]] ConnectionManager &connectionManager() noexcept;
	[[nodiscard]] ClientNetworkManager &networkManager() noexcept;
	[[nodiscard]] TerrainCollections &terrainCollections() noexcept;
};
