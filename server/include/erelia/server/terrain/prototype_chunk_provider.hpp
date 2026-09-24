#pragma once

#include "erelia/core/chunk_collection.hpp"

namespace erelia::server
{
	class PrototypeChunkProvider final : public Chunk::Collection::Provider
	{
	public:
		[[nodiscard]] Chunk provide(
			const Chunk::Coordinate &coordinate) override;
	};
}
