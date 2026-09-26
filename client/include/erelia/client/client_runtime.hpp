#pragma once

#include <cstdint>
#include <optional>
#include <string>

#include <network/client.hpp>
#include <threading/task.hpp>

class ClientRuntime final
{
public:
	struct Configuration
	{
		std::string address;
		std::uint16_t port = 0;

		[[nodiscard]] static Configuration load(
			const std::string &path);
	};

	using ConnectionTask = spk::Task<bool>;
	using ConnectionAnswer = ConnectionTask::Answer;

private:
	Configuration _configuration;
	spk::Client _client;
	std::optional<ConnectionAnswer> _connectionAttempt;

public:
	explicit ClientRuntime(Configuration configuration);
	ClientRuntime(const ClientRuntime &) = delete;
	ClientRuntime(ClientRuntime &&) = delete;
	ClientRuntime &operator=(const ClientRuntime &) = delete;
	ClientRuntime &operator=(ClientRuntime &&) = delete;
	~ClientRuntime();

	[[nodiscard]] ConnectionAnswer connect();
	void disconnect();

	[[nodiscard]] bool isConnected() const noexcept;
};
