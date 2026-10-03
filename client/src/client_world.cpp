#include "erelia/client/client_world.hpp"

#include "erelia/client/service.hpp"
#include "erelia/core/service.hpp"

#include <network/client.hpp>

#include <utility>

ClientWorld::ClientWorld(
	WorldIdentifier identifier,
	ClientNetworkManager &networkManager) :
	World(std::move(identifier)),
	_networkManager(networkManager)
{
	_disconnectContract =
		Service::clientEventCenter().clientDisconnected().subscribe(
			[this] {
				if (Chunks *chunks = _existingChunkCollection(); chunks != nullptr)
				{
					chunks->provider().disconnect();
				}
				if (Columns *columns = _existingColumnCollection(); columns != nullptr)
				{
					columns->provider().disconnect();
				}
			});
}

std::unique_ptr<World::Chunks> ClientWorld::_createChunkCollection()
{
	auto collection = std::make_unique<Chunks>(
		Chunks::RequestingProvider(
			Service::workerPool(),
			[](const spk::Message &message) {
				Service::client().send(message);
			}));

	_chunkUpdater =
		std::make_unique<Chunks::Updater>(*collection);
	_bind(*collection, *_chunkUpdater);

	return collection;
}

std::unique_ptr<World::Columns> ClientWorld::_createColumnCollection()
{
	auto collection = std::make_unique<Columns>(
		Columns::RequestingProvider(
			Service::workerPool(),
			[](const spk::Message &message) {
				Service::client().send(message);
			}));

	_columnUpdater =
		std::make_unique<Columns::Updater>(*collection);
	_bind(*collection, *_columnUpdater);

	return collection;
}

template <typename TKey, typename TElement>
void ClientWorld::_bind(
	Collection<TKey, TElement> &collection,
	typename Collection<TKey, TElement>::Updater &updater)
{
	using Types = typename TElement::Protocol::MessageTypes;
	auto &provider =
		static_cast<typename Collection<TKey, TElement>::RequestingProvider &>(
			collection.provider());

	_subscriptions.push_back(
		_networkManager.dispatcher().subscribe(
			static_cast<spk::Message::Type>(Types::Response),
			[&provider](const spk::Message &message) {
				provider.receive(message);
			}));
	_subscriptions.push_back(
		_networkManager.dispatcher().subscribe(
			static_cast<spk::Message::Type>(Types::Update),
			[&updater](const spk::Message &message) {
				updater.receive(message);
			}));
	_subscriptions.push_back(
		_networkManager.dispatcher().subscribe(
			static_cast<spk::Message::Type>(Types::Error),
			[&provider](const spk::Message &message) {
				provider.receiveError(message);
			}));
}
