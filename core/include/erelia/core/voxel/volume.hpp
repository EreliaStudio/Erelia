#pragma once

#include <cstddef>
#include <optional>
#include <span>
#include <vector>

#include <container/pool.hpp>
#include <math/vector3.hpp>
#include <network/message.hpp>

#include "erelia/core/voxel/cell.hpp"

namespace Voxel
{
	class Volume
	{
	public:
		using LocalCoordinate = spk::Vector3Int;
		using UnitSize = float;

		struct Buffer final : public std::vector<Cell>
		{
			using Base = std::vector<Cell>;
			using Pool = spk::Pool<Buffer>;
			using Lease = Pool::Lease;

			using Base::Base;
		};

		class Builder;

	private:
		spk::Vector3UInt _dimensions{};
		UnitSize _unitSize = 0.0f;
		Buffer::Lease _cells;

		Volume(
			const spk::Vector3UInt &dimensions,
			UnitSize unitSize,
			Buffer::Lease cells) noexcept;
		[[nodiscard]] static Buffer::Lease obtainCellBuffer(std::size_t expectedSize);
		[[nodiscard]] static bool canReuseCellBuffer(
			const Buffer::Lease &cells,
			std::size_t expectedSize);
		[[nodiscard]] std::size_t _index(const LocalCoordinate &coordinate) const;

		friend spk::Message &operator<<(
			spk::Message &message,
			const Volume &volume);
		friend const spk::Message &operator>>(
			const spk::Message &message,
			Volume &volume);

	public:
		Volume() = default;
		explicit Volume(const spk::Message &message);
		Volume(const Volume &other);
		Volume(Volume &&other) noexcept;
		~Volume() = default;

		Volume &operator=(const Volume &other);
		Volume &operator=(Volume &&other) noexcept;

		[[nodiscard]] spk::Vector3UInt dimensions() const noexcept;
		[[nodiscard]] UnitSize unitSize() const noexcept;
		[[nodiscard]] bool contains(const LocalCoordinate &coordinate) const noexcept;
		[[nodiscard]] std::optional<Cell> tryGet(const LocalCoordinate &coordinate) const noexcept;
		[[nodiscard]] Cell at(const LocalCoordinate &coordinate) const;
		[[nodiscard]] Cell operator[](const LocalCoordinate &coordinate) const;
		[[nodiscard]] std::span<const Cell> cells() const noexcept;
	};
}
