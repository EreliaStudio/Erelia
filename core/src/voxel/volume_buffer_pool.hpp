#pragma once

#include <cstddef>

#include "erelia/core/voxel/volume.hpp"

namespace Voxel
{
	[[nodiscard]] Volume::Buffer::Lease obtainCellBuffer(std::size_t expectedSize);
}
