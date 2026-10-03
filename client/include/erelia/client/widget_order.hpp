#pragma once

#include <ui/widget.hpp>

namespace Client::WidgetOrder
{
	inline constexpr spk::Widget::ZOrder Connection = -300.0f;
	inline constexpr spk::Widget::ZOrder Network = -200.0f;
	inline constexpr spk::Widget::ZOrder World = 0.0f;
	inline constexpr spk::Widget::ZOrder Interface = 100.0f;
}
