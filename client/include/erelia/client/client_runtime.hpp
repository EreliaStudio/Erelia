#pragma once

#include "erelia/client/client_network_manager.hpp"
#include "erelia/client/connection_manager.hpp"
#include "erelia/client/event_center.hpp"
#include "erelia/core/world.hpp"

#include <ui/widget.hpp>

class ClientRuntime final
{
private:
	ConnectionManager _connectionManager;
	ClientNetworkManager _networkManager;
	World *_world = nullptr;
	Core::Event<World *>::Contract _worldChangedContract;
	Core::Event<>::Contract _connectedContract;
	Core::Event<>::Contract _playerLoadingRequestedContract;

public:
	ClientRuntime(
		ConnectionManager::Endpoint endpoint,
		spk::Timer::Duration retryDelay,
		spk::Widget *parent);
	~ClientRuntime();

	[[nodiscard]] ConnectionManager &connectionManager() noexcept;
	[[nodiscard]] ClientNetworkManager &networkManager() noexcept;
	[[nodiscard]] World *world() noexcept;
	[[nodiscard]] const World *world() const noexcept;
};
