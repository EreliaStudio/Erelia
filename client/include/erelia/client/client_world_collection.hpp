#pragma once

#include "erelia/core/world_collection.hpp"

class ClientNetworkManager;

class ClientWorldCollection final : public WorldCollection
{
private:
	ClientNetworkManager *_networkManager = nullptr;

	[[nodiscard]] std::unique_ptr<World> _createWorld(
		const Identifier &identifier) override;

public:
	void bind(ClientNetworkManager &networkManager);
	void unbind(ClientNetworkManager &networkManager);
};
