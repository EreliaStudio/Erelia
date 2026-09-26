#pragma once

#include <network/message.hpp>

namespace Networking
{
	enum class MessageType : spk::Message::Type
	{
		ChunkRequest = 1,
		ChunkResponse = 2,
		ChunkError = 3,
		Diagnostic = 4
	};
}
