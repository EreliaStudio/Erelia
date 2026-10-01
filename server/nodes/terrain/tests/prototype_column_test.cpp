#include "prototype_chunk_provider.hpp"
#include <algorithm>
#include <gtest/gtest.h>
TEST(PrototypeTerrainColumns, SparseMembershipMatchesEveryGeneratedChunk)
{
	Collection<Column::Coordinate, Column> columns{PrototypeColumnProvider{}};
	Collection<Chunk::Coordinate, Chunk> chunks{PrototypeChunkProvider{}};
	for (const auto &[key, top] : std::vector<std::pair<Column::Coordinate, int>>{{{3, 3}, 1}, {{3, 4}, 2}, {{4, 3}, 2}, {{4, 4}, 3}, {{-1, -1}, 0}, {{1, 1}, 0}, {{2, 1}, 0}, {{1, 2}, 0}})
	{
		const Column column = columns.request(key).get();
		ASSERT_EQ(column.chunks.size(), static_cast<std::size_t>(top + 1));
		for (int y = -1; y <= top + 1; ++y)
		{
			const Chunk chunk = chunks.request(Chunk::Coordinate{key.x, y, key.z}).get();
			const bool nonempty = std::any_of(chunk.cells().begin(), chunk.cells().end(), [](const auto &cell) {
				return cell.definitionId() != 0;
			});
			EXPECT_EQ(nonempty, y >= 0 && y <= top);
			if (y > 0 && y <= top)
			{
				for (const auto &cell : chunk.cells())
				{
					EXPECT_EQ(cell.packed(), 1u);
				}
			}
		}
		for (int y = 0; y <= top; ++y)
		{
			EXPECT_EQ(column.chunks[static_cast<std::size_t>(y)], (Chunk::Coordinate{key.x, y, key.z}));
		}
	}
}
