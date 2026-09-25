#pragma once

#include <cstdint>
#include <string>

#include <network/remote_node.hpp>

class __NODE_NAME__Node final
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

public:
	explicit __NODE_NAME__Node(Configuration configuration);
	__NODE_NAME__Node(const __NODE_NAME__Node &) = delete;
	__NODE_NAME__Node(__NODE_NAME__Node &&) = delete;
	__NODE_NAME__Node &operator=(const __NODE_NAME__Node &) = delete;
	__NODE_NAME__Node &operator=(__NODE_NAME__Node &&) = delete;
	~__NODE_NAME__Node();

	void start();
	void stop();
	void dispatch();

	[[nodiscard]] bool isRunning() const noexcept;
	[[nodiscard]] std::uint16_t port() const noexcept;
};
