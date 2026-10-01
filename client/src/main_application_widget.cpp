#include "erelia/client/main_application_widget.hpp"

#include "erelia/client/service.hpp"
#include <network/client.hpp>
#include <utility>

MainApplicationWidget::MainApplicationWidget(
	ConnectionManager::Endpoint endpoint,
	spk::Timer::Duration retryDelay,
	spk::Widget *parent,
	TerrainStreamingBehaviour::Ranges ranges) :
	spk::Widget("/MainApplicationWidget", parent),
	_network(this),
	_ranges(ranges),
	_connectionManager(name() + "/ConnectionManager", std::move(endpoint), retryDelay, this),
	_console(name() + "/Console", this)
{
	auto &connectCommand =
		_console.commandParser().addCommand<ConnectCommand>();
	_connectRequestContract =
		connectCommand.subscribeToRequest(
			[this](const ConnectCommand::Request &request) {
				_connect(request);
			});

	_layout.addWidget(&_console, {spk::Layout::SizePolicy::Extend, spk::Layout::SizePolicy::Extend});
	activate();
}

void MainApplicationWidget::_connect(const ConnectCommand::Request &request)
{
	ConnectionManager::Endpoint endpoint = _connectionManager.endpoint();
	if (request.address.has_value() == true)
	{
		endpoint.address = *request.address;
	}
	if (request.port.has_value() == true)
	{
		endpoint.port = *request.port;
	}

	const ConnectionManager::Endpoint &current = _connectionManager.endpoint();
	if (
		current.address == endpoint.address &&
		current.port == endpoint.port)
	{
		_connectionManager.connect();
		return;
	}

	_connectionManager.connect(std::move(endpoint));
}

void MainApplicationWidget::_onGeometryChange()
{
	_layout.setGeometry(geometry());
}

ConnectionManager &MainApplicationWidget::connectionManager() noexcept
{
	return _connectionManager;
}

Console &MainApplicationWidget::console() noexcept
{
	return _console;
}

void MainApplicationWidget::_updateState(spk::UpdateContext &context)
{
	if (Service::client().isConnected() == true && _player == nullptr)
	{
		_player = std::make_unique<Player>(_terrain, _ranges);
	}
	if (_player != nullptr)
	{
		_player->updateState(context);
	}
}
