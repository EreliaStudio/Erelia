#pragma once

#include "erelia/client/client_network_manager.hpp"
#include "erelia/core/collection_requesting_provider.hpp"
#include "erelia/core/collection_updater.hpp"
#include "erelia/core/world.hpp"

#include <memory>
#include <vector>

class ClientWorld final : public World
{
private:
	ClientNetworkManager &_networkManager;
	std::unique_ptr<Chunks::Updater> _chunkUpdater;
	std::unique_ptr<Columns::Updater> _columnUpdater;
	std::vector<Networking::MessageDispatcher<>::Contract> _subscriptions;
	Core::Event<>::Contract _disconnectContract;

	[[nodiscard]] std::unique_ptr<Chunks> _createChunkCollection() override;
	[[nodiscard]] std::unique_ptr<Columns> _createColumnCollection() override;

	template <typename TKey, typename TElement>
	void _bind(
		Collection<TKey, TElement> &collection,
		typename Collection<TKey, TElement>::Updater &updater);

public:
	ClientWorld(WorldIdentifier identifier, ClientNetworkManager &networkManager);
};
