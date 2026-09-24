#include "erelia/core/chunk.hpp"

#include <utility>

#include <exception.hpp>

namespace
{
	[[nodiscard]] constexpr spk::Vector3UInt chunkDimensions() noexcept
	{
		return {
			static_cast<std::uint32_t>(Chunk::Extent),
			static_cast<std::uint32_t>(Chunk::Extent),
			static_cast<std::uint32_t>(Chunk::Extent)};
	}
}

Chunk::Chunk(Voxel::Volume::Buffer::Lease cells) :
	Voxel::Volume(chunkDimensions(), 1.0f, std::move(cells))
{
}

Chunk::Chunk(Voxel::Volume &&volume)
{
	if (volume.dimensions() != chunkDimensions())
	{
		throw spk::Exception("Chunk requires exactly 16x16x16 Voxel::Volume dimensions");
	}
	if (volume.unitSize() != 1.0f)
	{
		throw spk::Exception("Chunk requires Voxel::Volume unit size 1.0");
	}

	Voxel::Volume::operator=(std::move(volume));
}
