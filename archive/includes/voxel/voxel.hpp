#pragma once

#include <cstdint>

namespace voxel::Voxel
{
	using ID = std::uint32_t;

	enum class Orientation : std::uint32_t
	{
		PositiveZ = 0b0000'0000'0000'0000'0000'0000'0000'0000u,
		PositiveX = 0b0010'0000'0000'0000'0000'0000'0000'0000u,
		NegativeZ = 0b0100'0000'0000'0000'0000'0000'0000'0000u,
		NegativeX = 0b0110'0000'0000'0000'0000'0000'0000'0000u
	};

	enum class Flip : std::uint32_t
	{
		PositiveY = 0b0000'0000'0000'0000'0000'0000'0000'0000u,
		NegativeY = 0b1000'0000'0000'0000'0000'0000'0000'0000u
	};

	enum class Side : std::uint8_t
	{
		NegativeX,
		PositiveX,
		NegativeY,
		PositiveY,
		NegativeZ,
		PositiveZ,
		None
	};

	class Cell;
	struct Shape;
	struct Definition;
	template <typename TDefinition>
	class Catalog;
}
