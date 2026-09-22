#pragma once

#include <cstdint>

#include <math/vector3.hpp>

namespace erelia::core::terrain
{
	inline constexpr std::int32_t chunkExtent = 16;
	inline constexpr float cellWorldExtent = 1.0F;

	[[nodiscard]] spk::Vector3Int toChunkCoordinate(const spk::Vector3Int &globalCell) noexcept;
	[[nodiscard]] spk::Vector3Int toLocalCoordinate(const spk::Vector3Int &globalCell) noexcept;
}
