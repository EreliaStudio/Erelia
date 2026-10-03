#include "erelia/client/requesting_world_provider.hpp"

#include "erelia/client/client_network_manager.hpp"
#include "erelia/client/client_world.hpp"

#include <exception.hpp>

std::unique_ptr<World> RequestingWorldProvider::_acquire(
	const WorldIdentifier &identifier)
{
	if (_networkManager == nullptr)
	{
		throw spk::Exception(
			"RequestingWorldProvider has no bound ClientNetworkManager");
	}

	return std::make_unique<ClientWorld>(
		identifier,
		*_networkManager);
}

void RequestingWorldProvider::bind(
	ClientNetworkManager &networkManager)
{
	if (_networkManager != nullptr &&
		_networkManager != &networkManager)
	{
		throw spk::Exception(
			"RequestingWorldProvider is already bound");
	}

	_networkManager = &networkManager;
}

void RequestingWorldProvider::unbind(
	ClientNetworkManager &networkManager)
{
	if (_networkManager == &networkManager)
	{
		_networkManager = nullptr;
	}
}
