#pragma once
#include "erelia/core/chunk.hpp"
#include <compare>
#include <cstdint>
#include <vector>
struct Column
{
	struct Coordinate
	{
		std::int32_t x = 0;
		std::int32_t z = 0;
		auto operator<=>(const Coordinate &) const = default;
	};
	inline static constexpr std::size_t MaximumElementsPerRequest = 1024;
	inline static constexpr std::size_t ElementsPerResponseSection = 32;
	std::vector<Chunk::Coordinate> chunks;
};
namespace std
{
	template <>
	struct hash<Column::Coordinate>
	{
		size_t operator()(const Column::Coordinate &key) const noexcept
		{
			return hash<Chunk::Coordinate>{}({key.x, 0, key.z});
		}
	};
}
spk::Message::Writer &operator<<(spk::Message::Writer &writer, const Column &column);
const spk::Message::Reader &operator>>(const spk::Message::Reader &reader, Column &column);
