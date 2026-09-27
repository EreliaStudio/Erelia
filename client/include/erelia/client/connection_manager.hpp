#pragma once

#include <atomic>
#include <chrono>
#include <cstddef>
#include <cstdint>
#include <optional>
#include <string>

#include <network/client.hpp>
#include <threading/task.hpp>
#include <threading/worker_pool.hpp>
#include <ui/widget.hpp>

class ConnectionManager final : public spk::Widget
{
public:
	struct Endpoint
	{
		std::string address;
		std::uint16_t port = 0;
	};

	static constexpr std::size_t MaximumAttemptCount = 3;
	static constexpr std::chrono::seconds RetryDelay{15};

private:
	using ConnectionTask = spk::Task<bool>;
	using ConnectionAnswer = ConnectionTask::Answer;

	Endpoint _endpoint;
	spk::Client &_client;
	spk::WorkerPool &_workerPool;
	std::optional<ConnectionAnswer> _connectionAttempt;
	std::chrono::steady_clock::duration _retryElapsed{};
	std::size_t _attemptCount = 0;
	bool _retryScheduled = false;
	bool _cycleStopped = false;
	std::atomic_bool _disconnected{false};
	spk::Client::ConnectionContract _connectionContract;
	spk::Client::DisconnectionContract _disconnectionContract;

	void _launchAttempt();
	void _processAttempt();
	void _scheduleRetry();
	void _stopCycle();
	void _updateState(spk::UpdateContext &context) override;

public:
	ConnectionManager(std::string name, Endpoint endpoint, spk::Widget *parent = nullptr);
	~ConnectionManager();

	void connect();
	void connect(Endpoint endpoint);
	[[nodiscard]] const Endpoint &endpoint() const noexcept;
	[[nodiscard]] bool isCycleStopped() const noexcept;
	[[nodiscard]] std::size_t attemptCount() const noexcept;
};
