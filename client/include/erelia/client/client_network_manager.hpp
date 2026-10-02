#pragma once
#include "erelia/core/networking/message_dispatcher.hpp"
#include <ui/widget.hpp>
class ClientNetworkManager final : public spk::Widget
{
	Networking::MessageDispatcher<> _dispatcher;
	std::vector<spk::Message> _messages;
	void _updateState(spk::UpdateContext &) override;

public:
	explicit ClientNetworkManager(spk::Widget *parent = nullptr);
	~ClientNetworkManager() override;
	[[nodiscard]] Networking::MessageDispatcher<> &dispatcher() noexcept
	{
		return _dispatcher;
	}
	void dispatch();
};
