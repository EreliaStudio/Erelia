#include "erelia/client/main_application_widget.hpp"

#include "erelia/client/service.hpp"
#include <utility>

MainApplicationWidget::MainApplicationWidget(
	ConnectionManager::Endpoint endpoint,
	spk::Timer::Duration retryDelay,
	spk::Widget *parent,
	TerrainStreamingBehaviour::Ranges ranges) :
	spk::Widget("/MainApplicationWidget", parent),
	_ranges(ranges),
	_connectionManager(name() + "/ConnectionManager", std::move(endpoint), retryDelay, this),
	_network(this),
	_console(name() + "/Console", this)
{
	_connectedContract = Service::clientEventCenter().clientConnected().subscribe([this] {
		if (_player == nullptr)
		{
			_player = std::make_unique<Player>(_terrain, _ranges);
		}
	});
	_console.commandParser().addCommand<ConnectCommand>();

	_layout.addWidget(&_console, {spk::Layout::SizePolicy::Extend, spk::Layout::SizePolicy::Extend});
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

void MainApplicationWidget::_updateState(spk::UpdateContext &context)
{
	if (_player != nullptr)
	{
		_player->updateState(context);
	}
}
