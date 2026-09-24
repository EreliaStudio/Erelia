#pragma once

#include "erelia/core/chunk.hpp"
#include "erelia/core/voxel/volume_builder.hpp"

class Chunk::Builder final : public Voxel::Volume::Builder
{
public:
	Builder();

	[[nodiscard]] Chunk build() &&;
};
