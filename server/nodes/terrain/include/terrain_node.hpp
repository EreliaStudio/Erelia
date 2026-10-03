#pragma once
#include "erelia/core/networking/message_dispatcher.hpp"
#include "erelia/core/networking/terrain_protocol.hpp"
#include "terrain_world.hpp"
#include <cstdint>
#include <memory>
#include <network/remote_node.hpp>
#include <string>
#include <vector>
class TerrainNode final
{
public:
	using Request = spk::RemoteNode::Endpoint::Request;
	struct Configuration
	{
		std::uint16_t port = 0;
		[[nodiscard]] static Configuration load(const std::string &path);
	};

private:
	struct AsyncState;
	Configuration _configuration;
	spk::RemoteNode::Endpoint &_endpoint;
	TerrainWorld _world;
	Networking::MessageDispatcher<Request> _dispatcher;
	std::vector<Networking::MessageDispatcher<Request>::Contract> _subscriptions;
	std::vector<Request> _requests;
	std::unique_ptr<AsyncState> _async;
	template <typename TKey, typename TElement>
	void _request(const Request &request, Collection<TKey, TElement> &collection);
	void _drainCompletions();

public:
	explicit TerrainNode(Configuration configuration);
	TerrainNode(const TerrainNode &) = delete;
	TerrainNode &operator=(const TerrainNode &) = delete;
	~TerrainNode();
	void start();
	void stop();
	void dispatch();
	void reply(const Request &request, spk::Message message) noexcept;
	[[nodiscard]] bool isRunning() const noexcept;
	[[nodiscard]] std::uint16_t port() const noexcept;
};
