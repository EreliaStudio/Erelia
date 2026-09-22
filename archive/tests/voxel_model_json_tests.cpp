#include "voxel/voxel_model.hpp"

#include <string>
#include <string_view>

#include <exception.hpp>
#include <gtest/gtest.h>

namespace
{
	void expectInvalidModel(std::string_view json, std::string_view path)
	{
		const auto value = spk::JSON::Value::fromString(json);
		const spk::JSON::Reader reader(value, "model.json");
		try
		{
			const voxel::VoxelModel model(reader);
			FAIL() << "expected spk::Exception";
		}
		catch (const spk::Exception &exception)
		{
			EXPECT_NE(std::string(exception.what()).find(path), std::string::npos);
		}
	}
}

TEST(VoxelModelJSONTest, LoadsSparseValuesNamedOrientationsAndOptionalFlip)
{
	const auto value = spk::JSON::Value::fromString(R"({
		"dimensions": [5, 1, 1],
		"voxelSize": 0.1,
		"voxels": [
			{"position": [0, 0, 0], "value": 1, "orientation": "positive-z"},
			{"position": [1, 0, 0], "value": 2, "orientation": "positive-x"},
			{"position": [2, 0, 0], "value": 3, "orientation": "negative-z", "flip": "negative-y"},
			{"position": [3, 0, 0], "value": 4, "orientation": "negative-x", "flip": "positive-y"}
		]
	})");
	const spk::JSON::Reader reader(value, "model.json");
	const voxel::VoxelModel model(reader);

	EXPECT_EQ(model.dimensions(), spk::Vector3UInt(5, 1, 1));
	EXPECT_FLOAT_EQ(model.voxelSize(), 0.1f);
	EXPECT_EQ(model.at({0, 0, 0}), voxel::Voxel::Cell(1, voxel::Voxel::Orientation::PositiveZ));
	EXPECT_EQ(model.at({1, 0, 0}), voxel::Voxel::Cell(2, voxel::Voxel::Orientation::PositiveX));
	EXPECT_EQ(model.at({2, 0, 0}), voxel::Voxel::Cell(3, voxel::Voxel::Orientation::NegativeZ, voxel::Voxel::Flip::NegativeY));
	EXPECT_EQ(model.at({3, 0, 0}), voxel::Voxel::Cell(4, voxel::Voxel::Orientation::NegativeX));
	EXPECT_TRUE(model.at({4, 0, 0}).empty());
}

TEST(VoxelModelJSONTest, RejectsDuplicateAndOutOfRangePositions)
{
	expectInvalidModel(R"({
		"dimensions": [1, 1, 1], "voxelSize": 1.0,
		"voxels": [
			{"position": [0, 0, 0], "value": 1, "orientation": "positive-z"},
			{"position": [0, 0, 0], "value": 2, "orientation": "positive-x"}
		]
	})", "model.json:$.voxels[1].position:");
	expectInvalidModel(R"({
		"dimensions": [1, 1, 1], "voxelSize": 1.0,
		"voxels": [{"position": [1, 0, 0], "value": 1, "orientation": "positive-z"}]
	})", "model.json:$.voxels[0].position:");
}

TEST(VoxelModelJSONTest, RejectsUnknownOrientationAndFlipNames)
{
	expectInvalidModel(R"({
		"dimensions": [1, 1, 1], "voxelSize": 1.0,
		"voxels": [{"position": [0, 0, 0], "value": 1, "orientation": "up"}]
	})", "model.json:$.voxels[0].orientation:");
	expectInvalidModel(R"({
		"dimensions": [1, 1, 1], "voxelSize": 1.0,
		"voxels": [{"position": [0, 0, 0], "value": 1, "orientation": "positive-z", "flip": "sideways"}]
	})", "model.json:$.voxels[0].flip:");
}

TEST(VoxelModelJSONTest, RejectsValuesOutsidePackedCellCapacity)
{
	expectInvalidModel(R"({
		"dimensions": [1, 1, 1], "voxelSize": 1.0,
		"voxels": [{"position": [0, 0, 0], "value": 536870912, "orientation": "positive-z"}]
	})", "model.json:$.voxels[0].value:");
}

TEST(VoxelModelJSONTest, RejectsUnknownModelAndVoxelFields)
{
	expectInvalidModel(R"({
		"dimensions": [1, 1, 1], "voxelSize": 1.0, "name": "prototype", "voxels": []
	})", "model.json:$.name:");
	expectInvalidModel(R"({
		"dimensions": [1, 1, 1], "voxelSize": 1.0,
		"voxels": [{"position": [0, 0, 0], "value": 1, "orientation": "positive-z", "material": 2}]
	})", "model.json:$.voxels[0].material:");
}
