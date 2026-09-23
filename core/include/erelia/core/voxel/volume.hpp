#pragma once

#include <cstddef>
#include <memory>
#include <span>

#include <math/vector3.hpp>

#include "erelia/core/voxel/cell.hpp"

namespace Voxel
{
	class Volume
	{
	public:
		using LocalCoordinate = spk::Vector3Int;
		using UnitSize = float;

		class Builder;

	private:
		struct Content;

		std::shared_ptr<Content> _content;

		explicit Volume(std::shared_ptr<Content> content) noexcept;
		[[nodiscard]] std::size_t _index(const LocalCoordinate &coordinate) const;

	public:
		Volume() = default;
		Volume(const Volume &) = default;
		Volume(Volume &&) noexcept = default;
		~Volume() = default;

		Volume &operator=(const Volume &) = default;
		Volume &operator=(Volume &&) noexcept = default;

		[[nodiscard]] spk::Vector3UInt dimensions() const noexcept;
		[[nodiscard]] UnitSize unitSize() const noexcept;
		[[nodiscard]] bool contains(const LocalCoordinate &coordinate) const noexcept;
		[[nodiscard]] Cell at(const LocalCoordinate &coordinate) const;
		[[nodiscard]] Cell operator[](const LocalCoordinate &coordinate) const;
		[[nodiscard]] std::span<const Cell> cells() const noexcept;
	};
}
