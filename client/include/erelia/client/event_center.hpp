#pragma once

#include "erelia/core/event_center.hpp"

#include <cstdint>
#include <optional>
#include <string>

#include <math/vector3.hpp>

namespace Client
{
	struct ConnectionRequest
	{
		std::optional<std::string> address;
		std::optional<std::uint16_t> port;
	};

	class EventCenter
	{
		Core::Event<spk::Vector3Int> _playerChangedChunk;
		Core::Event<const ConnectionRequest &> _connectionRequested;
		Core::Event<> _clientDisconnected;
		Core::Event<> _clientConnected;

	public:
		[[nodiscard]] Core::Event<spk::Vector3Int> &playerChangedChunkEvent() noexcept
		{
			return _playerChangedChunk;
		}
		[[nodiscard]] Core::Event<const ConnectionRequest &> &connectionRequested() noexcept
		{
			return _connectionRequested;
		}
		[[nodiscard]] Core::Event<> &clientDisconnected() noexcept
		{
			return _clientDisconnected;
		}
		[[nodiscard]] Core::Event<> &clientConnected() noexcept
		{
			return _clientConnected;
		}
	};
}
