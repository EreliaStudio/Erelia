#include "current_chunk_golden_fixture.hpp"

#include <sparkle_test.hpp>

#include <array>
#include <filesystem>
#include <iostream>
#include <exception.hpp>
#include <string>

namespace playground_test::golden
{
	namespace
	{
		constexpr std::string_view CurrentChunkCategory = "current_chunk";
		constexpr std::string_view VoxelMesherCategory = "voxel_mesher";

		void require(bool condition, std::string_view message)
		{
			if (!condition) throw spk::Exception(std::string(message));
		}

		sparkle_test::ImageComparisonOptions comparisonOptions()
		{
			sparkle_test::ImageComparisonOptions options;
			require(options.rgbTolerance == 4, "Sparkle RGB tolerance default changed");
			require(options.alphaTolerance == 8, "Sparkle alpha tolerance default changed");
			require(options.transparentAlphaThreshold == 4, "Sparkle transparent-alpha threshold default changed");
			return options;
		}

		void preserveExpectedCandidate(
			std::string_view category, std::string_view name, const std::filesystem::path &actual)
		{
			const std::filesystem::path candidate =
				std::filesystem::path(PLAYGROUND_TEST_EXPECTED_CANDIDATE_DIR) / category / (std::string(name) + ".png");
			std::filesystem::create_directories(candidate.parent_path());
			std::filesystem::copy_file(actual, candidate, std::filesystem::copy_options::overwrite_existing);
			std::cerr << "Proposed expected image: " << candidate << '\n';
		}

		std::string viewName(std::string_view name, std::size_t viewIndex)
		{
			if (viewIndex == 0) return std::string(name);
			return std::string(name) + "_view_" + std::to_string(viewIndex + 1);
		}

		bool compareCandidate(
			std::string_view category, std::string_view name, const std::filesystem::path &actual)
		{
			const auto expected = sparkle_test::expectedImagePath(category, std::string(name));
			const auto difference = sparkle_test::resultImagePath(category, std::string(name) + "_difference");
			if (!std::filesystem::is_regular_file(expected))
			{
				preserveExpectedCandidate(category, name, actual);
				std::cerr << "Missing approved reference: " << expected << "\nCandidate preserved: " << actual << '\n';
				return false;
			}
			const auto result = sparkle_test::compareImages(actual, expected, difference, comparisonOptions());
			if (!result.matches)
			{
				std::cerr << "Fixture " << name << " differs: " << result.differentPixelCount << " pixels; actual "
					<< result.actualWidth << 'x' << result.actualHeight << "; expected "
					<< result.expectedWidth << 'x' << result.expectedHeight << '\n';
			}
			return result.matches;
		}
	}

	bool compareScene(std::string_view name)
	{
		const auto &fixture = scene(name);
		bool matches = true;
		for (std::size_t viewIndex = 0; viewIndex < ViewCount; ++viewIndex)
		{
			const std::string candidateName = viewName(name, viewIndex);
			const auto actual = sparkle_test::resultImagePath(CurrentChunkCategory, candidateName + "_actual");
			renderScene(fixture, viewIndex, actual);
			if (!compareCandidate(CurrentChunkCategory, candidateName, actual)) matches = false;
		}
		return matches;
	}

	bool compareSeededScene()
	{
		bool matches = true;
		for (std::size_t viewIndex = 0; viewIndex < ViewCount; ++viewIndex)
		{
			const std::string candidateName = viewName("seeded_debug_chunks", viewIndex);
			const auto actual = sparkle_test::resultImagePath(CurrentChunkCategory, candidateName + "_actual");
			renderSeededScene(viewIndex, actual);
			if (!compareCandidate(CurrentChunkCategory, candidateName, actual)) matches = false;
		}
		return matches;
	}

	bool compareVoxelMesherModels()
	{
		bool matches = true;
		for (std::size_t viewIndex = 0; viewIndex < ViewCount; ++viewIndex)
		{
			const std::string jsonName = viewName("json_cross_statue", viewIndex);
			const std::string volumeName = viewName("procedural_cross_statue", viewIndex);
			const auto jsonActual = sparkle_test::resultImagePath(VoxelMesherCategory, jsonName + "_actual");
			const auto volumeActual = sparkle_test::resultImagePath(VoxelMesherCategory, volumeName + "_actual");
			renderVoxelMesherModel(false, viewIndex, jsonActual);
			renderVoxelMesherModel(true, viewIndex, volumeActual);
			const auto parityJson = sparkle_test::resultImagePath(VoxelMesherCategory, jsonName + "_parity_json");
			const auto parityVolume = sparkle_test::resultImagePath(VoxelMesherCategory, jsonName + "_parity_volume");
			std::filesystem::copy_file(jsonActual, parityJson, std::filesystem::copy_options::overwrite_existing);
			std::filesystem::copy_file(volumeActual, parityVolume, std::filesystem::copy_options::overwrite_existing);
			const auto parityDifference = sparkle_test::resultImagePath(VoxelMesherCategory, jsonName + "_parity_difference");
			const auto parity = sparkle_test::compareImages(parityJson, parityVolume, parityDifference, comparisonOptions());
			if (!parity.matches) matches = false;
			else
			{
				std::filesystem::remove(parityJson);
				std::filesystem::remove(parityVolume);
			}
			if (!compareCandidate(VoxelMesherCategory, jsonName, jsonActual)) matches = false;
			if (!compareCandidate(VoxelMesherCategory, volumeName, volumeActual)) matches = false;
		}
		const std::string pairName = "json_and_procedural_cross_statues";
		const auto pairActual = sparkle_test::resultImagePath(VoxelMesherCategory, pairName + "_actual");
		renderVoxelMesherPair(pairActual);
		return compareCandidate(VoxelMesherCategory, pairName, pairActual) && matches;
	}

	bool compareChunkMesherOcclusion()
	{
		struct Fixture
		{
			std::string_view name;
			bool publishNeighbor;
			bool renderNeighbor;
		};
		constexpr std::array fixtures{
			Fixture{"chunk_left_without_neighbor", false, false},
			Fixture{"chunk_left_with_hidden_neighbor", true, false},
			Fixture{"chunk_both_neighbors_rendered", true, true}};
		bool matches = true;
		for (const auto &fixture : fixtures)
		{
			const auto actual = sparkle_test::resultImagePath(VoxelMesherCategory, std::string(fixture.name) + "_actual");
			renderChunkMesherOcclusion(fixture.publishNeighbor, fixture.renderNeighbor, actual);
			if (!compareCandidate(VoxelMesherCategory, fixture.name, actual)) matches = false;
		}
		return matches;
	}

	void proveUvSensitivity()
	{
		const auto &fixture = scene("solid_cube_boundary");
		const auto temporaryReference = sparkle_test::resultImagePath("sensitivity", "temporary_reference");
		const auto unchanged = sparkle_test::resultImagePath("sensitivity", "unchanged_actual");
		const auto altered = sparkle_test::resultImagePath("sensitivity", "altered_uv_actual");
		const auto restored = sparkle_test::resultImagePath("sensitivity", "restored_actual");
		const auto difference = sparkle_test::resultImagePath("sensitivity", "altered_uv_difference");
		renderScene(fixture, 0, unchanged);
		std::filesystem::create_directories(temporaryReference.parent_path());
		std::filesystem::copy_file(unchanged, temporaryReference, std::filesystem::copy_options::overwrite_existing);
		require(sparkle_test::compareImages(unchanged, temporaryReference,
			sparkle_test::resultImagePath("sensitivity", "unchanged_difference"), comparisonOptions()).matches,
			"unchanged sensitivity fixture did not match");
		renderScene(fixture, 0, altered, true);
		require(!sparkle_test::compareImages(altered, temporaryReference, difference, comparisonOptions()).matches,
			"controlled UV alteration was not detected");
		require(std::filesystem::is_regular_file(altered) && std::filesystem::is_regular_file(difference),
			"UV mismatch artifacts were not preserved");
		renderScene(fixture, 0, restored);
		require(sparkle_test::compareImages(restored, temporaryReference,
			sparkle_test::resultImagePath("sensitivity", "restored_difference"), comparisonOptions()).matches,
			"restored sensitivity fixture did not match");
	}
}
