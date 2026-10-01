#pragma once

#include "erelia/client/connection_manager.hpp"

#include "erelia/client/terrain_streaming_behaviour.hpp"

#include <chrono>
#include <filesystem>

struct ClientConfiguration
{
	ConnectionManager::Endpoint server;
	std::chrono::milliseconds retryDelay;
	TerrainStreamingBehaviour::Ranges terrain;

	[[nodiscard]] static ClientConfiguration load(const std::filesystem::path &path);
};
