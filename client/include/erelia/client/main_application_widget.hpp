#pragma once

#include "erelia/client/command/connect_command.hpp"
#include "erelia/client/connection_manager.hpp"
#include "erelia/client/console.hpp"

#include "erelia/client/event_center.hpp"
#include "erelia/client/terrain_streaming_behaviour.hpp"

#include <ui/layout/linear_layout.hpp>
#include <ui/widget.hpp>

class MainApplicationWidget final : public spk::Widget
{
private:
	TerrainStreamingBehaviour::Ranges _ranges;
	// Deliver lifecycle events before draining replies in the child update order.
	ConnectionManager _connectionManager;
	ClientNetworkManager _network;
	TerrainCollections _terrain{_network};
	std::unique_ptr<Player> _player;
	Console _console;
	Core::Event<>::Contract _connectedContract;
	spk::VerticalLayout _layout;

	void _updateState(spk::UpdateContext &context) override;
	void _onGeometryChange() override;

public:
	MainApplicationWidget(
		ConnectionManager::Endpoint endpoint,
		spk::Timer::Duration retryDelay,
		spk::Widget *parent,
		TerrainStreamingBehaviour::Ranges ranges);

	[[nodiscard]] ConnectionManager &connectionManager() noexcept;
	[[nodiscard]] Console &console() noexcept;
};
