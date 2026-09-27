#pragma once

#include "erelia/client/connection_manager.hpp"
#include "erelia/client/console.hpp"

#include <ui/layout/linear_layout.hpp>
#include <ui/widget.hpp>

class MainApplicationWidget final : public spk::Widget
{
private:
	ConnectionManager _connectionManager;
	Console _console;
	Console::ConnectRequestContract _connectRequestContract;
	spk::VerticalLayout _layout;

	void _connect(const Console::ConnectRequest &request);
	void _onGeometryChange() override;

public:
	MainApplicationWidget(
		ConnectionManager::Endpoint endpoint,
		spk::Timer::Duration retryDelay,
		spk::Widget *parent);

	[[nodiscard]] ConnectionManager &connectionManager() noexcept;
	[[nodiscard]] Console &console() noexcept;
};
