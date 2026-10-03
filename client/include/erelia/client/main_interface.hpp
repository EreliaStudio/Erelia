#pragma once

#include "erelia/client/console.hpp"

#include <ui/layout/linear_layout.hpp>
#include <ui/widget.hpp>

#include <string>

class MainInterface final : public spk::Widget
{
private:
	Console _console;
	spk::VerticalLayout _layout;

	void _onGeometryChange() override;

public:
	MainInterface(
		std::string name,
		spk::Widget *parent = nullptr);

	[[nodiscard]] Console &console() noexcept;
	[[nodiscard]] const Console &console() const noexcept;
};
