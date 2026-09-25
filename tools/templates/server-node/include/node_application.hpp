#pragma once

#include "__NODE_SNAKE___node.hpp"

#include <atomic>
#include <csignal>

class __NODE_NAME__NodeApplication final
{
private:
	__NODE_NAME__Node _node;
	std::atomic_bool _stopRequested{false};
	std::atomic_bool _running{false};

	inline static volatile std::sig_atomic_t _signalReceived = 0;

	static void _onSignal(int signal);

public:
	explicit __NODE_NAME__NodeApplication(
		__NODE_NAME__Node::Configuration configuration);

	__NODE_NAME__NodeApplication(const __NODE_NAME__NodeApplication &) = delete;
	__NODE_NAME__NodeApplication &operator=(const __NODE_NAME__NodeApplication &) = delete;
	__NODE_NAME__NodeApplication(__NODE_NAME__NodeApplication &&) = delete;
	__NODE_NAME__NodeApplication &operator=(__NODE_NAME__NodeApplication &&) = delete;

	void run();
	void stop() noexcept;

	[[nodiscard]] bool isRunning() const noexcept;
};

[[nodiscard]] int run__NODE_NAME__Node(int argc, char **argv);
