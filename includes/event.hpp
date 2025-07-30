#pragma once

#include <sparkle.hpp>

enum class Event
{
	MovePlayer
};

using EventDispatcher = spk::EventDispatcher<Event>;