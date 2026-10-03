#include "erelia/client/client_world_collection.hpp"

#include "erelia/client/client_network_manager.hpp"
#include "erelia/client/client_world.hpp"

#include <exception.hpp>

std::unique_ptr<World> ClientWorldCollection::_createWorld(
	const Identifier &)
{
	if (_networkManager == nullptr)
	{
		throw spk::Exception(
			"Client WorldCollection has no bound ClientNetworkManager");
	}

	return std::make_unique<ClientWorld>(
		*_networkManager);
}

void ClientWorldCollection::bind(
	ClientNetworkManager &networkManager)
{
	if (_networkManager != nullptr &&
		_networkManager != &networkManager)
	{
		throw spk::Exception(
			"Client WorldCollection is already bound");
	}

	_networkManager = &networkManager;
}

void ClientWorldCollection::unbind(
	ClientNetworkManager &networkManager)
{
	if (_networkManager != &networkManager)
	{
		return;
	}

	clear();
	_networkManager = nullptr;
}
