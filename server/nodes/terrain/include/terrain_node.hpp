#pragma once

#include <cstdint>
#include <string>

#include <network/remote_node.hpp>

#include "erelia/core/chunk_collection.hpp"

class TerrainNode final
{
public:
	struct Configuration
	{
		std::uint16_t port = 0;

		[[nodiscard]] static Configuration load(
			const std::string &path);
	};

private:
	Configuration _configuration;
	spk::RemoteNode::Endpoint _endpoint;
	Chunk::Collection _chunks;

public:
	explicit TerrainNode(Configuration configuration);
	TerrainNode(const TerrainNode &) = delete;
	TerrainNode(TerrainNode &&) = delete;
	TerrainNode &operator=(const TerrainNode &) = delete;
	TerrainNode &operator=(TerrainNode &&) = delete;
	~TerrainNode();

	void start();
	void stop();
	void dispatch();

	[[nodiscard]] bool isRunning() const noexcept;
	[[nodiscard]] std::uint16_t port() const noexcept;
};
