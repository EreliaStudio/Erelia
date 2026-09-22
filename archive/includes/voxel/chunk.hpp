#pragma once

#include <compare>
#include <cstdint>

#include "voxel/voxel_volume.hpp"

namespace voxel
{
	class Chunk final : public VoxelVolume
	{
	public:
		static constexpr std::int32_t Size = 16;
		static constexpr std::size_t CellCount = Size * Size * Size;

		struct Coordinate
		{
			std::int32_t x = 0;
			std::int32_t y = 0;
			std::int32_t z = 0;
			auto operator<=>(const Coordinate &) const = default;
		};

		struct CoordinateHash;
		class Collection;
		class Generator;
		class DebugGenerator;
		class Mesher;
		class BakeScheduler;
		class Requester;
		class View;
		class ViewCollection;

	private:
		Coordinate _coordinate;

	public:
		explicit Chunk(Coordinate coordinate);

		[[nodiscard]] Coordinate coordinate() const noexcept
		{
			return _coordinate;
		}
		[[nodiscard]] static bool contains(spk::Vector3Int local) noexcept;
		[[nodiscard]] static std::size_t index(spk::Vector3Int local);

		[[nodiscard]] static Coordinate worldToChunk(spk::Vector3Int world) noexcept;
		[[nodiscard]] static spk::Vector3Int worldToLocal(spk::Vector3Int world) noexcept;
		[[nodiscard]] static spk::Vector3Int worldOrigin(Coordinate coordinate) noexcept;
	};
}
