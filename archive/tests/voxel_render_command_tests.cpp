#include "rendering/command/voxel_render_command.hpp"

#include <gtest/gtest.h>

#include <memory>
#include <utility>

namespace
{
	voxel::VoxelMesh triangle(voxel::Palette::ElementIndex elementIndex)
	{
		voxel::VoxelMesh::Builder builder;
		const auto first = builder.addVertex({{0.0f, 0.0f, 0.0f}, {0.0f, 1.0f, 0.0f}, elementIndex});
		const auto second = builder.addVertex({{1.0f, 0.0f, 0.0f}, {0.0f, 1.0f, 0.0f}, elementIndex});
		const auto third = builder.addVertex({{0.0f, 0.0f, 1.0f}, {0.0f, 1.0f, 0.0f}, elementIndex});
		builder.addTriangle(first, second, third);
		return std::move(builder).build();
	}

	voxel::Palette palette(std::size_t size)
	{
		voxel::Palette result;
		result.resize(size);
		for (std::size_t index = 0; index < size; ++index)
			result.data(static_cast<voxel::Palette::ElementIndex>(index)).color = {1.0f, 1.0f, 1.0f, 1.0f};
		result.validate();
		return result;
	}
}

TEST(VoxelRenderCommandTest, StoresACopyThatSharesThePaletteGpuResource)
{
	std::unique_ptr<voxel::VoxelRenderCommand> command;
	{
		const auto temporary = palette(1);
		command = std::make_unique<voxel::VoxelRenderCommand>(
			temporary, triangle(0), spk::Matrix4x4::identity());
	}

	EXPECT_NE(command, nullptr);
}
