#pragma once
#include <design_pattern/contract_provider.hpp>
#include <memory>
#include <mutex>
#include <network/message.hpp>
#include <unordered_map>
namespace Networking
{
	template <typename TEnvelope = spk::Message>
	class MessageDispatcher
	{
		using Subscriptions = spk::ContractProvider<const TEnvelope &>;
		std::mutex _mutex;
		std::unordered_map<spk::Message::Type, std::shared_ptr<Subscriptions>> _subscriptions;

	public:
		using Contract = typename Subscriptions::Contract;
		[[nodiscard]] Contract subscribe(spk::Message::Type type, typename Subscriptions::callback_type callback)
		{
			std::shared_ptr<Subscriptions> subscriptions;
			{
				const std::scoped_lock lock(_mutex);
				auto &slot = _subscriptions[type];
				if (slot == nullptr)
				{
					slot = std::make_shared<Subscriptions>();
				}
				subscriptions = slot;
			}
			return subscriptions->subscribe(std::move(callback));
		}
		void dispatch(spk::Message::Type type, const TEnvelope &envelope)
		{
			std::shared_ptr<Subscriptions> subscriptions;
			{
				const std::scoped_lock lock(_mutex);
				auto found = _subscriptions.find(type);
				if (found == _subscriptions.end())
				{
					return;
				}
				subscriptions = found->second;
			}
			subscriptions->trigger(envelope);
		}
	};
}
