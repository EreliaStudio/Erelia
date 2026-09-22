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
		inline static constexpr std::uint32_t DefinitionMask = 0x1FFFFFFFu;
		inline static constexpr std::uint32_t OrientationMask = 0x60000000u;
		inline static constexpr std::uint32_t FlipOrientationMask = 0x80000000u;
		inline static constexpr std::uint32_t OrientationShift = 29u;
		inline static constexpr std::uint32_t FlipOrientationShift = 31u;

		std::uint32_t _packed = 0;

	public:
		static const Cell Empty;

		Cell() noexcept = default;
		explicit Cell(std::uint32_t packed) noexcept :
			_packed(packed)
		{
		}

		Cell(
			std::uint32_t definitionId,
			Orientation orientation,
			FlipOrientation flipOrientation);

		[[nodiscard]] std::uint32_t definitionId() const noexcept
		{
			return _packed & DefinitionMask;
		}

		[[nodiscard]] Orientation orientation() const noexcept
		{
			return static_cast<Orientation>(
				(_packed & OrientationMask) >> OrientationShift);
		}

		[[nodiscard]] FlipOrientation flipOrientation() const noexcept
		{
			return static_cast<FlipOrientation>(
				(_packed & FlipOrientationMask) >> FlipOrientationShift);
		}

		[[nodiscard]] std::uint32_t packed() const noexcept
		{
			return _packed;
		}
	};
}
