#include "erelia/core/chunk_builder.hpp"

#include <utility>

Chunk::Builder::Builder() :
	Voxel::Volume::Builder(
		{static_cast<std::uint32_t>(Chunk::Extent),
		 static_cast<std::uint32_t>(Chunk::Extent),
		 static_cast<std::uint32_t>(Chunk::Extent)},
		1.0f)
{
}

Chunk Chunk::Builder::build() &&
{
	return Chunk(std::move(_cells));
}
