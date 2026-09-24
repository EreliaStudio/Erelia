#pragma once

#include "voxel/voxel_world.hpp"

#include <array>
#include <cstddef>
#include <filesystem>
#include <string_view>
#include <vector>

namespace playground_test::golden
{
	inline constexpr std::size_t ViewCount = 4;

	struct Camera
	{
		spk::Vector3 position;
		spk::Vector3 target;
		float verticalFieldOfViewRadians;
		float nearPlane;
		float farPlane;
	};

	struct Cell
	{
		voxel::Chunk::Coordinate chunk;
		spk::Vector3Int local;
		std::string_view definition;
		voxel::Voxel::Orientation orientation = voxel::Voxel::Orientation::PositiveZ;
		voxel::Voxel::Flip flip = voxel::Voxel::Flip::PositiveY;
	};

	struct Scene
	{
		std::string_view name;
		std::array<Camera, ViewCount> cameras;
		std::vector<Cell> cells;
	};

	[[nodiscard]] const Scene &scene(std::string_view name);
	[[nodiscard]] const std::array<Camera, ViewCount> &seededSceneCameras();
	void configureTestPaths();
	void renderScene(const Scene &scene, std::size_t viewIndex, const std::filesystem::path &path, bool alterFirstUv = false);
	void renderSeededScene(std::size_t viewIndex, const std::filesystem::path &path);
	void renderVoxelMesherModel(bool procedural, std::size_t viewIndex, const std::filesystem::path &path);
	void renderVoxelMesherPair(const std::filesystem::path &path);
	void renderChunkMesherOcclusion(bool publishNeighbor, bool renderNeighbor, const std::filesystem::path &path);
	void renderPaletteModel(bool orcPalette, const std::filesystem::path &path);
	void renderPaletteModelPair(const std::filesystem::path &path);
	void renderWorldPaletteChunk(const std::filesystem::path &path);
	[[nodiscard]] bool compareScene(std::string_view name);
	[[nodiscard]] bool compareSeededScene();
	[[nodiscard]] bool compareVoxelMesherModels();
	[[nodiscard]] bool compareChunkMesherOcclusion();
	[[nodiscard]] bool comparePaletteGoldenImages();
	void proveUvSensitivity();
}
