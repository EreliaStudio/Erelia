#pragma once

#include <cstdint>

#include <math/vector3.hpp>

namespace Voxel
{
	struct Cell
	{
		using Coordinate = spk::Vector3Int;

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
		std::uint32_t _packed;

	public:
		static const Cell Empty;

		Cell() noexcept;
		explicit Cell(std::uint32_t packed) noexcept;
		Cell(
			std::uint32_t definitionId,
			Orientation orientation,
			FlipOrientation flipOrientation);

		[[nodiscard]] std::uint32_t definitionId() const noexcept;
		[[nodiscard]] Orientation orientation() const noexcept;
		[[nodiscard]] FlipOrientation flipOrientation() const noexcept;
		[[nodiscard]] std::uint32_t packed() const noexcept;
	};
}
