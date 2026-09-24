#include "voxel/voxel_mesh.hpp"

#include <gtest/gtest.h>

#include <utility>

TEST(VoxelMeshTest, ConfiguresPaletteElementAsIntegerVertexAttribute)
{
	voxel::VoxelMesh::Builder builder;
	const auto first = builder.addVertex({{0.0f, 0.0f, 0.0f}, {0.0f, 1.0f, 0.0f}, 2});
	const auto second = builder.addVertex({{1.0f, 0.0f, 0.0f}, {0.0f, 1.0f, 0.0f}, 2});
	const auto third = builder.addVertex({{0.0f, 0.0f, 1.0f}, {0.0f, 1.0f, 0.0f}, 2});
	builder.addTriangle(first, second, third);
	const auto mesh = std::move(builder).build();
	const auto attributes = mesh.layout().vertexBuffer().attributes();

	ASSERT_EQ(attributes.size(), 3);
	EXPECT_EQ(attributes[2].attribute.location, 2);
	EXPECT_EQ(attributes[2].attribute.type, spk::VertexBuffer::Attribute::Type::UnsignedInt);
	EXPECT_EQ(attributes[2].attribute.componentCount, 1);
	EXPECT_EQ(attributes[2].attribute.interpretation, spk::VertexBuffer::Interpretation::Integer);
}
