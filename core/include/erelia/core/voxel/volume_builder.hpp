#pragma once

#include "erelia/core/voxel/volume.hpp"

namespace Voxel
{
	class Volume::Builder final
	{
	private:
		spk::Vector3UInt _dimensions{};
		UnitSize _unitSize = 0.0f;
		CellBufferLease _cells;

	public:
		Builder(const spk::Vector3UInt &dimensions, UnitSize unitSize);
		explicit Builder(Volume &&volume) noexcept;

		Builder(const Builder &) = delete;
		Builder(Builder &&) noexcept = default;
		~Builder() = default;

		Builder &operator=(const Builder &) = delete;
		Builder &operator=(Builder &&) noexcept = default;

		bool set(const LocalCoordinate &coordinate, Cell value);
		[[nodiscard]] Volume build() && noexcept;
	};
}
