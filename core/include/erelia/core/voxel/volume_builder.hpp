#pragma once

#include <memory>

#include "erelia/core/voxel/volume.hpp"

namespace Voxel
{
	class Volume::Builder final
	{
	private:
		std::shared_ptr<Content> _content;

	public:
		Builder(const spk::Vector3UInt &dimensions, UnitSize unitSize);
		explicit Builder(Volume &&volume);

		Builder(const Builder &) = delete;
		Builder(Builder &&) noexcept = default;
		~Builder() = default;

		Builder &operator=(const Builder &) = delete;
		Builder &operator=(Builder &&) noexcept = default;

		bool set(const LocalCoordinate &coordinate, Cell value);
		[[nodiscard]] Volume build() && noexcept;
	};
}
