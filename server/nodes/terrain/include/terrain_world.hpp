#pragma once

#include "erelia/core/world.hpp"

class TerrainWorld final : public World
{
private:
	[[nodiscard]] std::unique_ptr<Chunks> _createChunkCollection() override;
	[[nodiscard]] std::unique_ptr<Columns> _createColumnCollection() override;
};
