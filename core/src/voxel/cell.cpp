#include "erelia/core/voxel/cell.hpp"

#include <exception.hpp>

namespace Voxel
{
	const Cell Cell::Empty{};

	Cell::Cell(
		std::uint32_t definitionId,
		Orientation orientation,
		FlipOrientation flipOrientation)
	{
		const auto orientationValue = static_cast<std::uint32_t>(orientation);
		const auto flipOrientationValue = static_cast<std::uint32_t>(flipOrientation);

		if (definitionId > DefinitionMask)
		{
			throw spk::Exception("Voxel::Cell Definition ID exceeds its 29-bit capacity");
		}

		if (orientationValue > 3u)
		{
			throw spk::Exception("Voxel::Cell Orientation is invalid");
		}

		if (flipOrientationValue > 1u)
		{
			throw spk::Exception("Voxel::Cell FlipOrientation is invalid");
		}

		_packed =
			definitionId |
			((orientationValue << OrientationShift) & OrientationMask) |
			((flipOrientationValue << FlipOrientationShift) & FlipOrientationMask);
	}
}
