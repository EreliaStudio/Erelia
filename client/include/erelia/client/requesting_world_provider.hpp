#pragma once

#include "erelia/core/world_service.hpp"

class ClientNetworkManager;

class RequestingWorldProvider final : public WorldProvider
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
