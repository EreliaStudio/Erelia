#pragma once

#include "erelia/client/event_center.hpp"
#include "erelia/core/world_collection.hpp"

namespace spk
{
	class Client;
	class Translator;
}

namespace Service
{
	[[nodiscard]] Client::EventCenter &clientEventCenter();
	[[nodiscard]] spk::Client &client();
	[[nodiscard]] spk::Translator &translator();
	[[nodiscard]] WorldCollection &clientWorldCollection();
}
