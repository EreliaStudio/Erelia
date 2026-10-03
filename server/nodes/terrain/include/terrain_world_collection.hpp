#pragma once

#include "erelia/core/world_collection.hpp"

class TerrainWorldCollection final : public WorldCollection
{
private:
	[[nodiscard]] std::unique_ptr<World> _createWorld(
		const Identifier &identifier) override;
};
