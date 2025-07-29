#pragma once

enum class Event
{
	MovePlayer
};

using EventDispatcher = spk::EventDispatcher<Event>;