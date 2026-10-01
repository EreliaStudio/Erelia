#pragma once
#include "erelia/core/networking/message_dispatcher.hpp"
#include <atomic>
#include <network/client.hpp>
#include <ui/widget.hpp>
class ClientNetworkManager final : public spk::Widget
{
	Networking::MessageDispatcher<> _dispatcher;
	spk::ContractProvider<> _disconnect;
	std::shared_ptr<std::atomic_bool> _disconnected = std::make_shared<std::atomic_bool>(false);
	spk::Client::DisconnectionContract _disconnectionContract;
	std::vector<spk::Message> _messages;
	void _updateState(spk::UpdateContext &) override;

public:
	explicit ClientNetworkManager(spk::Widget *parent = nullptr);
	~ClientNetworkManager() override;
	[[nodiscard]] Networking::MessageDispatcher<> &dispatcher() noexcept
	{
		return _dispatcher;
	}
	[[nodiscard]] spk::ContractProvider<>::Contract subscribeToDisconnection(spk::ContractProvider<>::callback_type callback)
	{
		return _disconnect.subscribe(std::move(callback));
	}
	void dispatch();
};
