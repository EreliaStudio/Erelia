#pragma once
#include "erelia/core/column.hpp"
#include "erelia/core/networking/collection_protocol.hpp"
namespace Networking
{
	template <>
	struct CollectionMessageTypes<Chunk>
	{
		inline static constexpr MessageType Request = MessageType::ChunkRequest;
		inline static constexpr MessageType Response = MessageType::ChunkResponse;
		inline static constexpr MessageType Update = MessageType::ChunkUpdate;
		inline static constexpr MessageType Error = MessageType::ChunkError;
	};
	template <>
	struct CollectionMessageTypes<Column>
	{
		inline static constexpr MessageType Request = MessageType::ColumnRequest;
		inline static constexpr MessageType Response = MessageType::ColumnResponse;
		inline static constexpr MessageType Update = MessageType::ColumnUpdate;
		inline static constexpr MessageType Error = MessageType::ColumnError;
	};
	using ChunkProtocol = CollectionProtocol<Chunk::Coordinate, Chunk>;
	using ColumnProtocol = CollectionProtocol<Column::Coordinate, Column>;
}
