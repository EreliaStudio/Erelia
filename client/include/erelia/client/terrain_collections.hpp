#pragma once
#include "erelia/client/client_network_manager.hpp"
#include "erelia/core/collection_requesting_provider.hpp"
#include "erelia/core/collection_updater.hpp"
#include "erelia/core/networking/terrain_protocol.hpp"
class TerrainCollections
{
public:
	using Chunks = Collection<Chunk::Coordinate, Chunk>;
	using Columns = Collection<Column::Coordinate, Column>;

private:
	Chunks _chunks;
	Columns _columns;
	Chunks::Updater _chunkUpdater{_chunks};
	Columns::Updater _columnUpdater{_columns};
	std::vector<Networking::MessageDispatcher<>::Contract> _subscriptions;
	spk::ContractProvider<>::Contract _disconnect;
	template <typename TKey, typename TElement>
	void _bind(ClientNetworkManager &manager, Collection<TKey, TElement> &collection, typename Collection<TKey, TElement>::Updater &updater);

public:
	explicit TerrainCollections(ClientNetworkManager &manager);
	[[nodiscard]] Chunks &chunks() noexcept
	{
		return _chunks;
	}
	[[nodiscard]] Columns &columns() noexcept
	{
		return _columns;
	}
};
