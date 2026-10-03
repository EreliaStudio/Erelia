#pragma once

#include "erelia/core/world_service.hpp"
#include "erelia/client/event_center.hpp"

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
	[[nodiscard]] WorldService &clientWorldService();
}
