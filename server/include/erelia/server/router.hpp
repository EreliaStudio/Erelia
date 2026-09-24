#pragma once

#include <chrono>
#include <cstdint>
#include <memory>
#include <string>
#include <string_view>
#include <vector>

#include <network/node_router.hpp>

class Router final
{
public:
	struct NodeConfiguration
	{
		std::string name;
		std::string address;
		std::uint16_t port = 0;
	};

	struct Configuration
	{
		std::uint16_t port = 0;
		std::chrono::milliseconds nodeReconnectDelay{0};
		std::vector<NodeConfiguration> nodes;

		[[nodiscard]] static Configuration load(
			const std::string &path);
	};

private:
	struct NodeState;

	Configuration _configuration;
	spk::NodeRouter _router;
	std::vector<std::unique_ptr<NodeState>> _nodes;

	void _attemptConnection(
		NodeState &node,
		std::chrono::steady_clock::time_point now);

public:
	explicit Router(Configuration configuration);
	Router(const Router &) = delete;
	Router(Router &&) = delete;
	Router &operator=(const Router &) = delete;
	Router &operator=(Router &&) = delete;
	~Router();

	void start();
	void stop();
	void dispatch();

	[[nodiscard]] bool isRunning() const noexcept;
	[[nodiscard]] std::uint16_t port() const noexcept;
	[[nodiscard]] bool isNodeConnected(std::string_view name) const noexcept;

	void redirect(
		spk::Message::Type messageType,
		std::string_view nodeName);
};
