#include "erelia/core/voxel/volume.hpp"

#include <cstdint>
#include <memory>
#include <utility>

#include <exception.hpp>

namespace Voxel
{
	Volume::Volume(
		const spk::Vector3UInt &dimensions,
		UnitSize unitSize,
		Buffer::Lease cells) :
		_dimensions(dimensions),
		_unitSize(unitSize),
		_cells(std::make_shared<Buffer::Lease>(std::move(cells)))
	{
	}

	Volume::Volume(const Volume &other) :
		_dimensions(other._dimensions),
		_unitSize(other._unitSize),
		_cells(other._cells)
	{
	}

	Volume::Volume(Volume &&other) noexcept :
		_dimensions(std::exchange(other._dimensions, {})),
		_unitSize(std::exchange(other._unitSize, 0.0f)),
		_cells(std::move(other._cells))
	{
	}

	Volume &Volume::operator=(const Volume &other)
	{
		if (this == &other)
		{
			return *this;
		}

		_dimensions = other._dimensions;
		_unitSize = other._unitSize;
		_cells = other._cells;

		return *this;
	}

	Volume &Volume::operator=(Volume &&other) noexcept
	{
		if (this == &other)
		{
			return *this;
		}

		_dimensions = std::exchange(other._dimensions, {});
		_unitSize = std::exchange(other._unitSize, 0.0f);
		_cells = std::move(other._cells);

		return *this;
	}

	std::size_t Volume::_index(const LocalCoordinate &coordinate) const
	{
		if (!contains(coordinate))
		{
			throw spk::Exception("Voxel::Volume coordinate is out of range");
		}

		const auto x = static_cast<std::size_t>(coordinate.x);
		const auto y = static_cast<std::size_t>(coordinate.y);
		const auto z = static_cast<std::size_t>(coordinate.z);
		const auto sizeX = static_cast<std::size_t>(_dimensions.x);
		const auto sizeY = static_cast<std::size_t>(_dimensions.y);

		return y + sizeY * (x + sizeX * z);
	}

	spk::Vector3UInt Volume::dimensions() const noexcept
	{
		return _dimensions;
	}

	Volume::UnitSize Volume::unitSize() const noexcept
	{
		return _unitSize;
	}

	bool Volume::contains(const LocalCoordinate &coordinate) const noexcept
	{
		return coordinate.x >= 0 && coordinate.y >= 0 && coordinate.z >= 0 && static_cast<std::uint32_t>(coordinate.x) < _dimensions.x && static_cast<std::uint32_t>(coordinate.y) < _dimensions.y && static_cast<std::uint32_t>(coordinate.z) < _dimensions.z;
	}

	std::optional<Cell> Volume::tryGet(const LocalCoordinate &coordinate) const noexcept
	{
		if (!contains(coordinate))
		{
			return std::nullopt;
		}

		const auto view = cells();
		return view[_index(coordinate)];
	}

	Cell Volume::at(const LocalCoordinate &coordinate) const
	{
		const auto view = cells();
		return view[_index(coordinate)];
	}

	Cell Volume::operator[](const LocalCoordinate &coordinate) const
	{
		return at(coordinate);
	}

	std::span<const Cell> Volume::cells() const noexcept
	{
		if (!_cells || !static_cast<bool>(*_cells))
		{
			return {};
		}

		const Buffer &cells = **_cells;
		return std::span<const Cell>(
			cells.data(),
			cells.size());
	}
}
