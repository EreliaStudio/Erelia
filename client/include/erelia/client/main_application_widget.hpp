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
	spk::VerticalLayout _layout;

	void _onGeometryChange() override;

public:
	MainApplicationWidget(ConnectionManager::Endpoint endpoint, spk::Widget *parent);

	[[nodiscard]] ConnectionManager &connectionManager() noexcept;
	[[nodiscard]] Console &console() noexcept;
};
