#include "erelia/core/voxel/volume_builder.hpp"

#include <cmath>
#include <limits>
#include <utility>

#include <exception.hpp>

namespace
{
	[[nodiscard]] std::size_t cellCount(const spk::Vector3UInt &dimensions)
	{
		if (dimensions.x == 0 || dimensions.y == 0 || dimensions.z == 0)
		{
			throw spk::Exception("Voxel::Volume dimensions must be strictly positive");
		}

		constexpr auto maximum = std::numeric_limits<std::size_t>::max();
		const auto sizeX = static_cast<std::size_t>(dimensions.x);
		const auto sizeY = static_cast<std::size_t>(dimensions.y);
		const auto sizeZ = static_cast<std::size_t>(dimensions.z);

		if (sizeY > maximum / sizeX || sizeZ > maximum / (sizeX * sizeY))
		{
			throw spk::Exception("Voxel::Volume cell count exceeds std::size_t capacity");
		}

		return sizeX * sizeY * sizeZ;
	}

	[[nodiscard]] Voxel::Volume::UnitSize validatedUnitSize(Voxel::Volume::UnitSize unitSize)
	{
		if (!std::isfinite(unitSize) || unitSize <= 0.0f)
		{
			throw spk::Exception("Voxel::Volume unit size must be finite and strictly positive");
		}

		return unitSize;
	}

	[[nodiscard]] std::size_t checkedIndex(
		const spk::Vector3UInt &dimensions,
		const Voxel::Volume::LocalCoordinate &coordinate)
	{
		const bool contained =
			coordinate.x >= 0 &&
			coordinate.y >= 0 &&
			coordinate.z >= 0 &&
			static_cast<std::uint32_t>(coordinate.x) < dimensions.x &&
			static_cast<std::uint32_t>(coordinate.y) < dimensions.y &&
			static_cast<std::uint32_t>(coordinate.z) < dimensions.z;

		if (!contained)
		{
			throw spk::Exception("Voxel::Volume::Builder coordinate is out of range");
		}

		const auto x = static_cast<std::size_t>(coordinate.x);
		const auto y = static_cast<std::size_t>(coordinate.y);
		const auto z = static_cast<std::size_t>(coordinate.z);
		const auto sizeX = static_cast<std::size_t>(dimensions.x);
		const auto sizeY = static_cast<std::size_t>(dimensions.y);

		return y + sizeY * (x + sizeX * z);
	}
}

namespace Voxel
{
	Volume::Builder::Builder(
		const spk::Vector3UInt &dimensions,
		UnitSize unitSize) :
		_dimensions(dimensions),
		_unitSize(validatedUnitSize(unitSize)),
		_cells(Volume::_obtainEmptyCellBuffer(dimensions, cellCount(dimensions)))
	{
	}

	Volume::Builder::Builder(Volume &&volume) noexcept :
		_dimensions(std::exchange(volume._dimensions, {})),
		_unitSize(std::exchange(volume._unitSize, 0.0f)),
		_cells(std::move(volume._cells))
	{
	}

	bool Volume::Builder::set(const LocalCoordinate &coordinate, Cell value)
	{
		const auto index = checkedIndex(_dimensions, coordinate);
		auto &cell = (*_cells)[index];

		if (cell.packed() == value.packed())
		{
			return false;
		}

		cell = value;
		return true;
	}

	Volume Volume::Builder::build() && noexcept
	{
		return Volume(
			std::exchange(_dimensions, {}),
			std::exchange(_unitSize, 0.0f),
			std::move(_cells));
	}
}
