#include "voxel_mesher_fixture.hpp"

#include <array>
#include <filesystem>

#include <sparkle>

namespace playground_test
{
	namespace
	{
		struct Cell
		{
			spk::Vector3Int position;
			voxel::Voxel::Cell value;
		};

		const std::array<Cell, MesherFixtureCellCount> Cells{{
			{{3, 0, 3}, voxel::Voxel::Cell(3)}, {{3, 1, 3}, voxel::Voxel::Cell(3)},
			{{3, 2, 3}, voxel::Voxel::Cell(3)}, {{3, 3, 3}, voxel::Voxel::Cell(3)},
			{{3, 4, 3}, voxel::Voxel::Cell(3)}, {{3, 5, 3}, voxel::Voxel::Cell(3)},
			{{1, 4, 3}, voxel::Voxel::Cell(3)}, {{2, 4, 3}, voxel::Voxel::Cell(3)},
			{{4, 4, 3}, voxel::Voxel::Cell(3)}, {{5, 4, 3}, voxel::Voxel::Cell(3)},
			{{0, 0, 0}, voxel::Voxel::Cell(1)},
			{{7, 0, 0}, voxel::Voxel::Cell(4, voxel::Voxel::Orientation::NegativeZ)},
			{{0, 0, 15}, voxel::Voxel::Cell(5, voxel::Voxel::Orientation::PositiveZ, voxel::Voxel::Flip::NegativeY)},
			{{7, 0, 15}, voxel::Voxel::Cell(6, voxel::Voxel::Orientation::PositiveX)},
			{{7, 3, 15}, voxel::Voxel::Cell(7, voxel::Voxel::Orientation::NegativeX, voxel::Voxel::Flip::NegativeY)}}};

		void fill(voxel::VoxelVolume &volume)
		{
			auto editor = volume.edit();
			for (const auto &cell : Cells) editor.set(cell.position, cell.value);
		}
	}

	std::unique_ptr<voxel::VoxelModel> loadVoxelMesherModel()
	{
		const auto path = std::filesystem::path(PLAYGROUND_TEST_RESOURCE_DIR) /
			"voxel_models" / "mesher_cross_statue.json";
		const auto value = spk::JSON::Loader::parseFile(path);
		return std::make_unique<voxel::VoxelModel>(spk::JSON::Reader(value, path));
	}

	std::unique_ptr<voxel::VoxelVolume> makeVoxelMesherVolume()
	{
		auto volume = std::make_unique<voxel::VoxelVolume>(spk::Vector3UInt{8, 8, 16}, 0.1f);
		fill(*volume);
		return volume;
	}
}
