#pragma once

#include "erelia/core/event_center.hpp"
#include <math/vector3.hpp>

namespace Client
{
	class EventCenter
	{
		Core::Event<spk::Vector3Int> _playerChangedChunk;
		Core::Event<> _clientDisconnected;
		Core::Event<> _clientConnected;

	public:
		[[nodiscard]] Core::Event<spk::Vector3Int> &playerChangedChunkEvent() noexcept
		{
			return _playerChangedChunk;
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
