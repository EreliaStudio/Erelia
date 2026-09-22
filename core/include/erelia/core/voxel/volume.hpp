#pragma once

#include <cstddef>
#include <span>
#include <vector>

#include <design_pattern/trait/versioned_trait.hpp>
#include <math/vector3.hpp>

#include "erelia/core/voxel/cell.hpp"

namespace Voxel
{
	class Volume : public spk::VersionedTrait
	{
	public:
		using LocalCoordinate = spk::Vector3Int;
		using UnitSize = float;

		class Editor;

	private:
		spk::Vector3UInt _dimensions{};
		UnitSize _unitSize = 0.0f;
		std::vector<Cell> _cells;

		[[nodiscard]] static std::size_t _cellCount(const spk::Vector3UInt &dimensions);
		[[nodiscard]] static UnitSize _validatedUnitSize(UnitSize unitSize);
		[[nodiscard]] std::size_t _index(const LocalCoordinate &coordinate) const;
		void _resetToDefault() noexcept;

	public:
		Volume() = default;
		Volume(const spk::Vector3UInt &dimensions, UnitSize unitSize);
		Volume(const Volume &other);
		Volume(Volume &&other);
		~Volume() override = default;

		Volume &operator=(const Volume &other);
		Volume &operator=(Volume &&other);

		[[nodiscard]] spk::Vector3UInt dimensions() const noexcept;
		[[nodiscard]] UnitSize unitSize() const noexcept;
		[[nodiscard]] bool contains(const LocalCoordinate &coordinate) const noexcept;
		[[nodiscard]] Cell at(const LocalCoordinate &coordinate) const;
		[[nodiscard]] std::span<const Cell> cells() const noexcept;
		[[nodiscard]] Editor edit();
	};

	class Volume::Editor final
	{
	private:
		Volume *_volume;
		bool _changed = false;

		friend class Volume;

		explicit Editor(Volume &volume) noexcept;

	public:
		Editor(Editor &&other) noexcept;
		Editor(const Editor &) = delete;
		~Editor();

		Editor &operator=(const Editor &) = delete;
		Editor &operator=(Editor &&) = delete;

		bool set(const LocalCoordinate &coordinate, Cell value);
		void commit();
	};
}
