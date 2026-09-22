#include "current_chunk_golden_fixture.hpp"

#include <algorithm>
#include <exception.hpp>

namespace playground_test::golden
{
	namespace
	{
		Camera rotatedCamera(const Camera &source, std::size_t quarterTurns)
		{
			Camera result = source;
			const float dx = source.position.x - source.target.x;
			const float dz = source.position.z - source.target.z;
			switch (quarterTurns % ViewCount)
			{
			case 1: result.position.x = source.target.x - dz; result.position.z = source.target.z + dx; break;
			case 2: result.position.x = source.target.x - dx; result.position.z = source.target.z - dz; break;
			case 3: result.position.x = source.target.x + dz; result.position.z = source.target.z - dx; break;
			default: break;
			}
			return result;
		}

		std::array<Camera, ViewCount> cameraViews(const Camera &primary)
		{
			return {rotatedCamera(primary, 0), rotatedCamera(primary, 1),
				rotatedCamera(primary, 2), rotatedCamera(primary, 3)};
		}

		Scene solidCubeBoundary()
		{
			return {"solid_cube_boundary", cameraViews(Camera{{24.0f, 18.0f, 28.0f}, {16.0f, 1.0f, 1.0f}, 0.72f, 0.1f, 100.0f}),
				{{{0, 0, 0}, {14, 0, 0}, "stone"}, {{0, 0, 0}, {15, 0, 0}, "stone"},
					{{0, 0, 0}, {14, 0, 1}, "stone"}, {{0, 0, 0}, {15, 0, 1}, "stone"},
					{{1, 0, 0}, {0, 0, 0}, "stone"}, {{1, 0, 0}, {0, 0, 1}, "stone"}}};
		}

		Scene slabPartialOcclusion()
		{
			using Flip = voxel::Voxel::Flip;
			using Orientation = voxel::Voxel::Orientation;
			return {"slab_partial_occlusion", cameraViews(Camera{{8.0f, 7.0f, 10.0f}, {1.5f, 0.5f, 0.5f}, 0.62f, 0.1f, 50.0f}),
				{{{0, 0, 0}, {0, 0, 0}, "debug_slab"}, {{0, 0, 0}, {1, 0, 0}, "stone"},
					{{0, 0, 0}, {3, 0, 0}, "debug_slab", Orientation::PositiveZ, Flip::NegativeY}}};
		}

		Scene slopeStairTransforms()
		{
			using Flip = voxel::Voxel::Flip;
			using Orientation = voxel::Voxel::Orientation;
			return {"slope_stair_transforms", cameraViews(Camera{{12.0f, 11.0f, 15.0f}, {4.5f, 1.0f, 2.5f}, 0.72f, 0.1f, 80.0f}),
				{{{0, 0, 0}, {0, 0, 0}, "debug_slope", Orientation::PositiveZ},
					{{0, 0, 0}, {2, 0, 0}, "debug_slope", Orientation::PositiveX},
					{{0, 0, 0}, {4, 0, 0}, "debug_slope", Orientation::NegativeZ, Flip::NegativeY},
					{{0, 0, 0}, {6, 0, 0}, "debug_slope", Orientation::NegativeX, Flip::NegativeY},
					{{0, 0, 0}, {0, 0, 3}, "debug_stair", Orientation::PositiveZ},
					{{0, 0, 0}, {2, 0, 3}, "debug_stair", Orientation::PositiveX},
					{{0, 0, 0}, {4, 0, 3}, "debug_stair", Orientation::NegativeZ, Flip::NegativeY},
					{{0, 0, 0}, {6, 0, 3}, "debug_stair", Orientation::NegativeX, Flip::NegativeY}}};
		}

		Scene materialsAndVegetation()
		{
			using Orientation = voxel::Voxel::Orientation;
			return {"materials_and_vegetation", cameraViews(Camera{{10.0f, 8.0f, 12.0f}, {2.5f, 1.0f, 1.5f}, 0.68f, 0.1f, 60.0f}),
				{{{0, 0, 0}, {0, 0, 0}, "grass"}, {{0, 0, 0}, {1, 0, 0}, "dirt"},
					{{0, 0, 0}, {2, 0, 0}, "stone"}, {{0, 0, 0}, {0, 1, 2}, "plant"},
					{{0, 0, 0}, {2, 1, 2}, "plant", Orientation::PositiveX}}};
		}

		std::vector<Scene> declaredScenes()
		{
			return {solidCubeBoundary(), slabPartialOcclusion(), slopeStairTransforms(), materialsAndVegetation()};
		}
	}

	const Scene &scene(std::string_view name)
	{
		static const auto scenes = declaredScenes();
		const auto found = std::ranges::find(scenes, name, &Scene::name);
		if (found == scenes.end()) throw spk::Exception("unknown current Chunk golden fixture");
		return *found;
	}

	const std::array<Camera, ViewCount> &seededSceneCameras()
	{
		static const auto cameras =
			cameraViews(Camera{{26.0f, 24.0f, 31.0f}, {0.0f, 5.0f, 0.0f}, 0.90f, 0.1f, 150.0f});
		return cameras;
	}
}
