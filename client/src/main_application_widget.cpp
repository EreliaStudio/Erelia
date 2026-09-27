#include "erelia/client/main_application_widget.hpp"

#include <utility>

MainApplicationWidget::MainApplicationWidget(
	ConnectionManager::Endpoint endpoint,
	spk::Widget *parent) :
	spk::Widget("MainApplicationWidget", parent),
	_connectionManager("ConnectionManager", std::move(endpoint), this),
	_console("Console", this),
	_connectRequestContract(_console.subscribeToConnectRequest([this](const Console::ConnectRequest &request) {
		_connect(request);
	}))
{
	_layout.addWidget(
		&_connectionManager,
		{spk::Layout::SizePolicy::Extend, spk::Layout::SizePolicy::Fixed});
	_layout.addWidget(&_console);
	activate();
}

void MainApplicationWidget::_connect(const Console::ConnectRequest &request)
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
