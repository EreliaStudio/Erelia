#pragma once
#include "erelia/core/column.hpp"
#include "erelia/core/networking/collection_protocol.hpp"
namespace Networking
{
	using ChunkProtocol = CollectionProtocol::Codec<Chunk::Coordinate, Chunk>;
	using ColumnProtocol = CollectionProtocol::Codec<Column::Coordinate, Column>;
}
