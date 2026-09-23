#pragma once

#include <utility>
#include <vector>

#include <container/pool.hpp>

#include "erelia/core/voxel/volume.hpp"

namespace Voxel
{
	struct Volume::Content
	{
		using CellBuffer = std::vector<Cell>;
		using CellBufferPool = spk::Pool<CellBuffer>;

		spk::Vector3UInt dimensions;
		UnitSize unitSize;
		CellBufferPool::Lease cells;

		Content(
			const spk::Vector3UInt &dimensions,
			UnitSize unitSize,
			CellBufferPool::Lease cells) :
			dimensions(dimensions),
			unitSize(unitSize),
			cells(std::move(cells))
		{
		}
	};
}
