#pragma once
#include <network/message.hpp>
#define COLLECTION_MESSAGES(Name) Name##Request, Name##Response, Name##Update, Name##Error
namespace Networking
{
	enum class MessageType : spk::Message::Type
	{
		Invalid = 0,
		COLLECTION_MESSAGES(Chunk),
		COLLECTION_MESSAGES(Column)
	};
}
