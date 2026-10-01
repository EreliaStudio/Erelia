#pragma once
#include "erelia/core/networking/collection_protocol.hpp"
template <typename TKey, typename TElement>
class Collection<TKey, TElement>::Updater
{
	Collection &_collection;

public:
	explicit Updater(Collection &collection) :
		_collection(collection)
	{
	}
	void receive(const spk::Message &message)
	{
		using Protocol = Networking::CollectionProtocol<TKey, TElement>;
		const typename Protocol::Update update(message);
		const auto state = _collection._provider->_state;
		for (std::size_t index = 0; index < update.sectionCount(); ++index)
		{
			const auto section = update.section(index, true);
			{
				const std::scoped_lock lock(state->mutex);
				for (const auto &entry : section.success)
				{
					auto found = state->pending.find(entry.key);
					if (found != state->pending.end())
					{
						Provider::_settle(state, entry.key, found->second.task, entry.element, nullptr);
					}
					else
					{
						state->storage->write()->insert_or_assign(entry.key, entry.element);
					}
				}
			}
			for (const auto &key : section.removed)
			{
				_collection.remove(key);
			}
		}
		_collection._provider->reclaim();
	}
};
