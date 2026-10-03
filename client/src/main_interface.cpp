#include "erelia/client/main_interface.hpp"

#include "erelia/client/widget_order.hpp"

#include <utility>

MainInterface::MainInterface(
	std::string name,
	spk::Widget *parent) :
	spk::Widget(std::move(name), parent),
	_console(this->name() + "/Console", this)
{
	setZOrder(Client::WidgetOrder::Interface);
	_layout.addWidget(
		&_console,
		{spk::Layout::SizePolicy::Extend, spk::Layout::SizePolicy::Extend});
	activate();
}

void MainInterface::_onGeometryChange()
{
	_layout.setGeometry(geometry());
}

Console &MainInterface::console() noexcept
{
	return _console;
}

const Console &MainInterface::console() const noexcept
{
	return _console;
}
