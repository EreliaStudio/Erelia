#include "erelia/core/voxel/volume.hpp"
#include "erelia/core/voxel/volume_editor.hpp"

#include <cmath>
#include <exception.hpp>
#include <limits>
#include <utility>

namespace
{
	std::size_t cellCount(const spk::Vector3UInt &dimensions)
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

	Voxel::Volume::UnitSize validatedUnitSize(Voxel::Volume::UnitSize unitSize)
	{
		if (!std::isfinite(unitSize) || unitSize <= 0.0f)
		{
			throw spk::Exception("Voxel::Volume unit size must be finite and strictly positive");
		}

		return unitSize;
	}
}

namespace Voxel
{

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

	void Volume::_resetToDefault() noexcept
	{
		_dimensions = {};
		_unitSize = 0.0f;
		_cells.clear();
	}

	Volume::Volume(const spk::Vector3UInt &dimensions, UnitSize unitSize) :
		_dimensions(dimensions),
		_unitSize(validatedUnitSize(unitSize)),
		_cells(cellCount(dimensions))
	{
	}

	Volume::Volume(const Volume &other) :
		spk::VersionedTrait(),
		_dimensions(other._dimensions),
		_unitSize(other._unitSize),
		_cells(other._cells)
	{
	}

	Volume::Volume(Volume &&other) :
		spk::VersionedTrait(),
		_dimensions(other._dimensions),
		_unitSize(other._unitSize),
		_cells(std::move(other._cells))
	{
		other._resetToDefault();
		other.invalidate();
	}

	Volume &Volume::operator=(const Volume &other)
	{
		if (this == &other)
		{
			return *this;
		}

		auto replacementCells = other._cells;
		const auto replacementDimensions = other._dimensions;
		const auto replacementUnitSize = other._unitSize;

		_dimensions = replacementDimensions;
		_unitSize = replacementUnitSize;
		_cells = std::move(replacementCells);
		invalidate();

		return *this;
	}

	Volume &Volume::operator=(Volume &&other)
	{
		if (this == &other)
		{
			return *this;
		}

		_dimensions = other._dimensions;
		_unitSize = other._unitSize;
		_cells = std::move(other._cells);
		other._resetToDefault();

		invalidate();
		other.invalidate();

		return *this;
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

	Cell Volume::at(const LocalCoordinate &coordinate) const
	{
		return _cells[_index(coordinate)];
	}

	Cell Volume::operator[](const LocalCoordinate &coordinate) const
	{
		return at(coordinate);
	}

	std::span<const Cell> Volume::cells() const noexcept
	{
		return _cells;
	}

	Volume::Editor Volume::edit()
	{
		return Editor(*this);
	}

	Volume::Editor::Editor(Volume &volume) noexcept :
		_volume(&volume)
	{
	}

	Volume::Editor::Editor(Editor &&other) noexcept :
		_volume(std::exchange(other._volume, nullptr)),
		_changed(std::exchange(other._changed, false))
	{
	}

	Volume::Editor::~Editor()
	{
		commit();
	}

	bool Volume::Editor::set(const LocalCoordinate &coordinate, Cell value)
	{
		if (_volume == nullptr)
		{
			throw spk::Exception("Voxel::Volume::Editor has already been committed");
		}

		auto &cell = _volume->_cells[_volume->_index(coordinate)];
		if (cell.packed() == value.packed())
		{
			return false;
		}

		cell = value;
		_changed = true;
		return true;
	}

	void Volume::Editor::commit()
	{
		if (_volume == nullptr)
		{
			return;
		}

		auto *volume = std::exchange(_volume, nullptr);
		if (_changed)
		{
			volume->invalidate();
		}
	}
}
