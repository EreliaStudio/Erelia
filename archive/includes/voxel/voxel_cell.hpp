#pragma once

#include <cstdint>
#include <type_traits>

#include <exception.hpp>

#include "voxel/voxel.hpp"

namespace voxel::Voxel
{
	class Cell final
	{
	public:
		static constexpr std::uint32_t OrientationMask = 0b0110'0000'0000'0000'0000'0000'0000'0000u;
		static constexpr std::uint32_t FlipMask = 0b1000'0000'0000'0000'0000'0000'0000'0000u;
		static constexpr std::uint32_t IDMask = ~(OrientationMask | FlipMask);

	private:
		std::uint32_t _value = 0;

	public:
		constexpr Cell() noexcept = default;
		constexpr Cell(ID id, Orientation orientation = Orientation::PositiveZ, Flip flip = Flip::PositiveY) :
			_value((id & IDMask) | static_cast<std::uint32_t>(orientation) | static_cast<std::uint32_t>(flip))
		{
			if (id > IDMask)
			{
				throw spk::Exception("voxel runtime identifier exceeds packed capacity");
			}
		}

		[[nodiscard]] constexpr ID id() const noexcept
		{
			return _value & IDMask;
		}
		[[nodiscard]] constexpr Orientation orientation() const noexcept
		{
			return static_cast<Orientation>(_value & OrientationMask);
		}
		[[nodiscard]] constexpr Flip flip() const noexcept
		{
			return static_cast<Flip>(_value & FlipMask);
		}
		[[nodiscard]] constexpr bool empty() const noexcept
		{
			return id() == 0;
		}
		[[nodiscard]] constexpr std::uint32_t packed() const noexcept
		{
			return _value;
		}

		[[nodiscard]] static constexpr Cell fromPacked(std::uint32_t value) noexcept
		{
			Cell result;
			result._value = value;
			return result;
		}

		constexpr bool operator==(const Cell &) const noexcept = default;
	};

	static_assert(sizeof(Cell) == sizeof(std::uint32_t));
	static_assert(std::is_trivially_copyable_v<Cell>);
}
