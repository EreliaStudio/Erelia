#pragma once

#include "erelia/core/networking/message_type.hpp"

namespace Networking
{
	struct CollectionProtocol
	{
		template <MessageType TRequest, MessageType TResponse, MessageType TUpdate, MessageType TError>
		struct MessageTypes
		{
			inline static constexpr MessageType Request = TRequest;
			inline static constexpr MessageType Response = TResponse;
			inline static constexpr MessageType Update = TUpdate;
			inline static constexpr MessageType Error = TError;
		};

		template <typename TKey, typename TElement>
		struct Codec;
	};
}
