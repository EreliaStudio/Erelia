#pragma once

#include "erelia/client/connection_manager.hpp"

#include <chrono>
#include <string>

struct ClientConfiguration
{
	ConnectionManager::Endpoint server;
	std::chrono::milliseconds retryDelay;

	[[nodiscard]] static ClientConfiguration load(const std::string &path);
};
