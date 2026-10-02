#include "erelia/client/terrain_collections.hpp"
#include "erelia/client/service.hpp"
#include "erelia/core/service.hpp"
#include <network/client.hpp>
TerrainCollections::TerrainCollections(ClientNetworkManager &manager) :
	_chunks(Chunks::RequestingProvider(Service::workerPool(), [](const spk::Message &message) {
		Service::client().send(message);
	})),
	_columns(Columns::RequestingProvider(Service::workerPool(), [](const spk::Message &message) {
		Service::client().send(message);
	}))
{
	_bind(manager, _chunks, _chunkUpdater);
	_bind(manager, _columns, _columnUpdater);
	_disconnect = Service::clientEventCenter().clientDisconnected().subscribe([this] {
		_chunks.provider().disconnect();
		_columns.provider().disconnect();
	});
}
template <typename TKey, typename TElement>
void TerrainCollections::_bind(ClientNetworkManager &manager, Collection<TKey, TElement> &collection, typename Collection<TKey, TElement>::Updater &updater)
{
	using Types = typename TElement::Protocol::MessageTypes;
	auto &provider = static_cast<typename Collection<TKey, TElement>::RequestingProvider &>(collection.provider());
	_subscriptions.push_back(manager.dispatcher().subscribe(static_cast<spk::Message::Type>(Types::Response), [&provider](const spk::Message &message) {
		provider.receive(message);
	}));
	_subscriptions.push_back(manager.dispatcher().subscribe(static_cast<spk::Message::Type>(Types::Update), [&updater](const spk::Message &message) {
		updater.receive(message);
	}));
	_subscriptions.push_back(manager.dispatcher().subscribe(static_cast<spk::Message::Type>(Types::Error), [&provider](const spk::Message &message) {
		provider.receiveError(message);
	}));
}
