#pragma once

#include <cstddef>
#include <span>
#include <vector>

#include <container/pool.hpp>
#include <math/vector3.hpp>

#include "erelia/core/voxel/cell.hpp"

namespace Voxel
{
	class Volume
	{
	public:
		using LocalCoordinate = spk::Vector3Int;
		using UnitSize = float;

	private:
		using CellBuffer = std::vector<Cell>;
		using CellBufferPool = spk::Pool<CellBuffer>;
		using CellBufferLease = CellBufferPool::Lease;

	public:
		class Builder;

	private:
		spk::Vector3UInt _dimensions{};
		UnitSize _unitSize = 0.0f;
		CellBufferLease _cells;

		Volume(
			const spk::Vector3UInt &dimensions,
			UnitSize unitSize,
			CellBufferLease cells) noexcept;
		[[nodiscard]] std::size_t _index(const LocalCoordinate &coordinate) const;

	public:
		Volume() = default;
		Volume(const Volume &other);
		Volume(Volume &&other) noexcept;
		~Volume() = default;

		Volume &operator=(const Volume &other);
		Volume &operator=(Volume &&other) noexcept;

		[[nodiscard]] spk::Vector3UInt dimensions() const noexcept;
		[[nodiscard]] UnitSize unitSize() const noexcept;
		[[nodiscard]] bool contains(const LocalCoordinate &coordinate) const noexcept;
		[[nodiscard]] Cell at(const LocalCoordinate &coordinate) const;
		[[nodiscard]] Cell operator[](const LocalCoordinate &coordinate) const;
		[[nodiscard]] std::span<const Cell> cells() const noexcept;
	};
}
