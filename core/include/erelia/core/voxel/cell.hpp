#pragma once

#include <cstdint>

#include <math/vector3.hpp>

#include "erelia/core/voxel/definition.hpp"

namespace Voxel
{
	struct Cell
	{
		using Coordinate = spk::Vector3Int;
		using PackedType = std::uint32_t;

		enum class Orientation : std::uint8_t
		{
			PositiveX = 0,
			NegativeX = 1,
			PositiveZ = 2,
			NegativeZ = 3
		};

		enum class FlipOrientation : std::uint8_t
		{
			PositiveY = 0,
			NegativeY = 1
		};

	private:
		PackedType _packed;

	public:
		static const Cell Empty;

		Cell() noexcept;
		explicit Cell(PackedType packed) noexcept;
		Cell(
			Definition::ID definitionId,
			Orientation orientation,
			FlipOrientation flipOrientation);

		[[nodiscard]] Definition::ID definitionId() const noexcept;
		[[nodiscard]] Orientation orientation() const noexcept;
		[[nodiscard]] FlipOrientation flipOrientation() const noexcept;
		[[nodiscard]] PackedType packed() const noexcept;
	};
}
