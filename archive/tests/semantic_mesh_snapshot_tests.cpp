#include "semantic_mesh_snapshot.hpp"

#include <gtest/gtest.h>

#include <array>
#include <utility>

namespace
{
	const std::array<spk::Texture3DVertex, 3> TriangleVertices{{
		{{0.0f, 0.0f, 0.0f}, {0.0f, 0.0f, 1.0f}, {0.0f, 0.0f}},
		{{1.0f, 0.0f, 0.0f}, {0.0f, 0.0f, 1.0f}, {1.0f, 0.0f}},
		{{0.0f, 1.0f, 0.0f}, {0.0f, 0.0f, 1.0f}, {0.0f, 1.0f}}}};

	spk::TextureMesh3D triangleMesh(std::array<std::size_t, 3> order)
	{
		spk::TextureMesh3D::Builder builder;
		std::array<spk::TextureMesh3D::Index, 3> indices;
		for (std::size_t index = 0; index < order.size(); ++index)
		{
			indices[index] = builder.addVertex(TriangleVertices[order[index]]);
		}
		builder.addTriangle(indices[0], indices[1], indices[2]);
		return std::move(builder).build();
	}
}

TEST(SemanticMeshSnapshotTest, IgnoresTriangleAndVertexBufferOrdering)
{
	const auto original = triangleMesh({0, 1, 2});
	const auto cyclicallyRotated = triangleMesh({2, 0, 1});
	EXPECT_EQ(playground_test::semanticMeshSnapshot(original),
		playground_test::semanticMeshSnapshot(cyclicallyRotated));
}

TEST(SemanticMeshSnapshotTest, PreservesVisibleWinding)
{
	const auto original = triangleMesh({0, 1, 2});
	const auto reversed = triangleMesh({0, 2, 1});
	EXPECT_NE(playground_test::semanticMeshSnapshot(original),
		playground_test::semanticMeshSnapshot(reversed));
}
