#include "erelia/client/main_application_widget.hpp"

#include <utility>

MainApplicationWidget::MainApplicationWidget(
	ConnectionManager::Endpoint endpoint,
	spk::Widget *parent) :
	spk::Widget("MainApplicationWidget", parent),
	_connectionManager("ConnectionManager", std::move(endpoint), this),
	_console("Console", _connectionManager, this)
{
	_layout.addWidget(
		&_connectionManager,
		{spk::Layout::SizePolicy::Extend, spk::Layout::SizePolicy::Fixed});
	_layout.addWidget(&_console);
	activate();
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
