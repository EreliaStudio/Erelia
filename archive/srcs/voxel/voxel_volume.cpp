#include "voxel/voxel_volume.hpp"

#include <cmath>
#include <exception.hpp>
#include <limits>

namespace voxel
{
	std::size_t VoxelVolume::_cellCount(spk::Vector3UInt dimensions)
	{
		if (dimensions.x == 0 || dimensions.y == 0 || dimensions.z == 0)
			throw spk::Exception("voxel volume dimensions must be positive");
		constexpr auto maximum = std::numeric_limits<std::size_t>::max();
		const auto width = static_cast<std::size_t>(dimensions.x);
		const auto height = static_cast<std::size_t>(dimensions.y);
		const auto depth = static_cast<std::size_t>(dimensions.z);
		if (height > maximum / width || depth > maximum / (width * height))
			throw spk::Exception("voxel volume cell count is not representable");
		return width * height * depth;
	}

	float VoxelVolume::_validatedVoxelSize(float voxelSize)
	{
		if (!std::isfinite(voxelSize) || voxelSize <= 0.0f)
			throw spk::Exception("voxel volume size must be finite and positive");
		return voxelSize;
	}

	VoxelVolume::VoxelVolume(spk::Vector3UInt dimensions, float voxelSize) :
		_dimensions(dimensions),
		_voxelSize(_validatedVoxelSize(voxelSize)),
		_cells(_cellCount(dimensions))
	{
	}

	spk::Vector3UInt VoxelVolume::dimensions() const noexcept
	{
		return _dimensions;
	}

	float VoxelVolume::voxelSize() const noexcept
	{
		return _voxelSize;
	}

	VoxelVolume::Bounds VoxelVolume::localBounds() const noexcept
	{
		return {{}, {_dimensions.x * _voxelSize, _dimensions.y * _voxelSize, _dimensions.z * _voxelSize}};
	}

	bool VoxelVolume::contains(spk::Vector3Int coordinate) const noexcept
	{
		return coordinate.x >= 0 && coordinate.y >= 0 && coordinate.z >= 0 &&
			static_cast<std::uint32_t>(coordinate.x) < _dimensions.x &&
			static_cast<std::uint32_t>(coordinate.y) < _dimensions.y &&
			static_cast<std::uint32_t>(coordinate.z) < _dimensions.z;
	}

	std::size_t VoxelVolume::_index(spk::Vector3Int coordinate) const
	{
		if (!contains(coordinate))
			throw spk::Exception("voxel volume coordinate is out of range");
		return (static_cast<std::size_t>(coordinate.y) * _dimensions.z + coordinate.z) * _dimensions.x + coordinate.x;
	}

	Voxel::Cell VoxelVolume::at(spk::Vector3Int coordinate) const
	{
		return _cells[_index(coordinate)];
	}

	std::span<const Voxel::Cell> VoxelVolume::cells() const noexcept
	{
		return _cells;
	}

	VoxelVolume::Editor VoxelVolume::edit()
	{
		return Editor(*this);
	}
}
