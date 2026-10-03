#pragma once

#include <core/platform/timer.hpp>
#include <network/client.hpp>
#include <threading/task.hpp>
#include <ui/widget.hpp>

#include <atomic>
#include <cstddef>
#include <cstdint>
#include <optional>
#include <string>

class ConnectionManager final : public spk::Widget
{
public:
	struct Endpoint
	{
		std::string address;
		std::uint16_t port = 0;
	};

	static constexpr std::size_t MaximumAttemptCount = 3;

private:
	using ConnectionTask = spk::Task<bool>;
	using ConnectionAnswer = ConnectionTask::Answer;

	Endpoint _endpoint;
	std::optional<ConnectionAnswer> _connectionAttempt;
	spk::Timer _retryTimer;
	std::size_t _attemptCount = 0;
	std::atomic_bool _disconnected = false;
	spk::Client::DisconnectionContract _disconnectionContract;
	// Transport callbacks only flag the event; processing stays on the Client update thread.
	void _processDisconnection();

	void _launchAttempt();
	void _processAttempt();
	void _scheduleRetry();
	void _stopCycle();
	void _updateState(spk::UpdateContext &context) override;

public:
	ConnectionManager(
		std::string name,
		Endpoint endpoint,
		spk::Timer::Duration retryDelay,
		spk::Widget *parent = nullptr);
	~ConnectionManager();

	void connect();
	void connect(Endpoint endpoint);
	[[nodiscard]] const Endpoint &endpoint() const noexcept;
	[[nodiscard]] bool isCycleStopped() const noexcept;
	[[nodiscard]] std::size_t attemptCount() const noexcept;
};
