#pragma once

#include <cstdint>
#include <memory>
#include <string>
#include <vector>

#include <network/remote_node.hpp>

#include "erelia/core/chunk_collection.hpp"

class TerrainNode final
{
public:
	using Request = spk::RemoteNode::Endpoint::Request;
	using RequestQueue = spk::RemoteNode::Endpoint::RequestQueue;

	struct Configuration
	{
		std::uint16_t port = 0;

		[[nodiscard]] static Configuration load(
			const std::string &path);
	};

private:
	struct AsyncState;

	Configuration _configuration;
	spk::RemoteNode::Endpoint _endpoint;
	Chunk::Collection _chunks;
	std::unique_ptr<AsyncState> _async;

	void _drainCompletions();

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

	void requestChunks(
		Request request,
		std::vector<Chunk::Coordinate> coordinates) noexcept;
	void reply(
		const Request &request,
		spk::Message message) noexcept;

	[[nodiscard]] bool isRunning() const noexcept;
	[[nodiscard]] std::uint16_t port() const noexcept;
	[[nodiscard]] RequestQueue &requests() noexcept;
};
