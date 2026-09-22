#include "current_chunk_golden_fixture.hpp"

#include <gtest/gtest.h>

namespace
{
	class CurrentChunkGoldenTest : public ::testing::Test
	{
	protected:
		static void SetUpTestSuite()
		{
			playground_test::golden::configureTestPaths();
		}
	};
}

TEST_F(CurrentChunkGoldenTest, SolidCubeBoundary)
{
	EXPECT_TRUE(playground_test::golden::compareScene("solid_cube_boundary"));
}

TEST_F(CurrentChunkGoldenTest, SlabPartialOcclusion)
{
	EXPECT_TRUE(playground_test::golden::compareScene("slab_partial_occlusion"));
}

TEST_F(CurrentChunkGoldenTest, SlopeAndStairTransforms)
{
	EXPECT_TRUE(playground_test::golden::compareScene("slope_stair_transforms"));
}

TEST_F(CurrentChunkGoldenTest, MaterialsAndVegetation)
{
	EXPECT_TRUE(playground_test::golden::compareScene("materials_and_vegetation"));
}

TEST_F(CurrentChunkGoldenTest, SeededDebugChunks)
{
	EXPECT_TRUE(playground_test::golden::compareSeededScene());
}

TEST_F(CurrentChunkGoldenTest, VoxelMesherJsonAndProceduralVolumes)
{
	EXPECT_TRUE(playground_test::golden::compareVoxelMesherModels());
}

TEST_F(CurrentChunkGoldenTest, ChunkMesherCrossChunkOcclusion)
{
	EXPECT_TRUE(playground_test::golden::compareChunkMesherOcclusion());
}

TEST_F(CurrentChunkGoldenTest, MatchesApprovedPaletteBaselines)
{
	EXPECT_TRUE(playground_test::golden::comparePaletteGoldenImages());
}

TEST_F(CurrentChunkGoldenTest, DetectsControlledUvRegression)
{
	EXPECT_NO_THROW(playground_test::golden::proveUvSensitivity());
}
