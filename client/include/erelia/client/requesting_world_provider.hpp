#pragma once

#include "erelia/core/world_collection.hpp"

class ClientNetworkManager;

class RequestingWorldProvider final : public WorldCollection::Provider
{
private:
	ClientNetworkManager *_networkManager = nullptr;

protected:
	[[nodiscard]] std::unique_ptr<World> _acquire(
		const WorldIdentifier &identifier) override;

public:
	void bind(ClientNetworkManager &networkManager);
	void unbind(ClientNetworkManager &networkManager);
};
