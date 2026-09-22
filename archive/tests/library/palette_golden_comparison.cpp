#include "current_chunk_golden_fixture.hpp"

#include <sparkle_test.hpp>

#include <array>
#include <filesystem>
#include <iostream>
#include <string>
#include <string_view>
#include <utility>

#include <exception.hpp>

namespace playground_test::golden
{
	namespace
	{
		constexpr std::string_view PaletteCategory = "palette_migration";

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

		bool compareApproved(std::string_view name, const std::filesystem::path &actual)
		{
			const auto expected = sparkle_test::expectedImagePath(PaletteCategory, std::string(name));
			const auto difference = sparkle_test::resultImagePath(PaletteCategory, std::string(name) + "_difference");
			if (!std::filesystem::is_regular_file(expected))
			{
				std::cerr << "Missing approved Palette reference: " << expected << '\n';
				return false;
			}
			const auto result = sparkle_test::compareImages(actual, expected, difference, comparisonOptions());
			if (!result.matches)
			{
				std::cerr << "Palette fixture " << name << " differs: " << result.differentPixelCount
					<< " pixels; actual " << result.actualWidth << 'x' << result.actualHeight << "; expected "
					<< result.expectedWidth << 'x' << result.expectedHeight << '\n';
			}
			return result.matches;
		}
	}

	bool comparePaletteGoldenImages()
	{
		const auto human = sparkle_test::resultImagePath(PaletteCategory, "human_palette_model_actual");
		const auto orc = sparkle_test::resultImagePath(PaletteCategory, "orc_palette_model_actual");
		const auto pair = sparkle_test::resultImagePath(PaletteCategory, "human_and_orc_palettes_actual");
		const auto world = sparkle_test::resultImagePath(PaletteCategory, "world_palette_chunk_actual");
		renderPaletteModel(false, human);
		renderPaletteModel(true, orc);
		renderPaletteModelPair(pair);
		renderWorldPaletteChunk(world);
		const auto oldExpected = sparkle_test::expectedImagePath("voxel_mesher", "json_cross_statue");
		const auto difference = sparkle_test::resultImagePath(PaletteCategory, "textured_to_palette_difference");
		if (!std::filesystem::is_regular_file(oldExpected))
			throw spk::Exception("approved textured reference is missing");
		if (sparkle_test::compareImages(human, oldExpected, difference).matches)
			throw spk::Exception("Palette candidate unexpectedly matches the textured reference");
		if (!std::filesystem::is_regular_file(human) || !std::filesystem::is_regular_file(difference))
			throw spk::Exception("Palette migration evidence was not preserved");
		bool matches = true;
		for (const auto &[name, actual] : std::array{
				std::pair{"human_palette_model", human},
				std::pair{"orc_palette_model", orc},
				std::pair{"human_and_orc_palettes", pair},
				std::pair{"world_palette_chunk", world}})
		{
			if (!compareApproved(name, actual)) matches = false;
		}
		return matches;
	}
}
