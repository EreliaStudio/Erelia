#include "erelia/core/voxel/volume.hpp"

#include <exception.hpp>

#include "volume_content.hpp"

namespace Voxel
{
	Volume::Volume(std::shared_ptr<Content> content) noexcept :
		_content(std::move(content))
	{
	}

	std::size_t Volume::_index(const LocalCoordinate &coordinate) const
	{
		if (!contains(coordinate))
		{
			throw spk::Exception("Voxel::Volume coordinate is out of range");
		}

		const auto dimensionsValue = dimensions();
		const auto x = static_cast<std::size_t>(coordinate.x);
		const auto y = static_cast<std::size_t>(coordinate.y);
		const auto z = static_cast<std::size_t>(coordinate.z);
		const auto sizeX = static_cast<std::size_t>(dimensionsValue.x);
		const auto sizeY = static_cast<std::size_t>(dimensionsValue.y);

		return y + sizeY * (x + sizeX * z);
	}

	spk::Vector3UInt Volume::dimensions() const noexcept
	{
		return _content != nullptr ? _content->dimensions : spk::Vector3UInt{};
	}

	Volume::UnitSize Volume::unitSize() const noexcept
	{
		return _content != nullptr ? _content->unitSize : 0.0f;
	}

	bool Volume::contains(const LocalCoordinate &coordinate) const noexcept
	{
		if (_content == nullptr)
		{
			return false;
		}

		return coordinate.x >= 0 && coordinate.y >= 0 && coordinate.z >= 0 && static_cast<std::uint32_t>(coordinate.x) < _content->dimensions.x && static_cast<std::uint32_t>(coordinate.y) < _content->dimensions.y && static_cast<std::uint32_t>(coordinate.z) < _content->dimensions.z;
	}

	Cell Volume::at(const LocalCoordinate &coordinate) const
	{
		return (*_content->cells)[_index(coordinate)];
	}

	Cell Volume::operator[](const LocalCoordinate &coordinate) const
	{
		return at(coordinate);
	}

	std::span<const Cell> Volume::cells() const noexcept
	{
		if (_content == nullptr)
		{
			return {};
		}

		return std::span<const Cell>(
			_content->cells->data(),
			_content->cells->size());
	}
}
