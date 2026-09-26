#pragma once

#include "terrain_node.hpp"

#include <atomic>
#include <csignal>
#include <vector>

class TerrainNodeApplication final
{
private:
	TerrainNode _node;
	std::vector<TerrainNode::Request> _requests;
	std::atomic_bool _stopRequested{false};
	std::atomic_bool _running{false};

	inline static volatile std::sig_atomic_t _signalReceived = 0;

	static void _onSignal(int signal);

	void _treatMessages();
	void _treatMessage(TerrainNode::Request request);
	void _parseChunkRequest(TerrainNode::Request request);

public:
	explicit TerrainNodeApplication(
		TerrainNode::Configuration configuration);

	TerrainNodeApplication(const TerrainNodeApplication &) = delete;
	TerrainNodeApplication &operator=(const TerrainNodeApplication &) = delete;
	TerrainNodeApplication(TerrainNodeApplication &&) = delete;
	TerrainNodeApplication &operator=(TerrainNodeApplication &&) = delete;

	void run();
	void stop() noexcept;

	[[nodiscard]] bool isRunning() const noexcept;
};

[[nodiscard]] int runTerrainNode(int argc, char **argv);
