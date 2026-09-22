#include <filesystem>
#include <string>

#include <gtest/gtest.h>

#include "rendering/command/clear_render_command.hpp"
#include "sparkle_test.hpp"

namespace
{
	class ClientGoldenImageTest : public ::testing::Test
	{
	protected:
		static void SetUpTestSuite()
		{
			sparkle_test::configurePaths(
				std::filesystem::path{ERELIA_CLIENT_TEST_RESOURCES_DIR},
				std::filesystem::path{ERELIA_CLIENT_TEST_RESULTS_DIR});
		}

		void expectSolidFrame(spk::Vector2UInt size, spk::Color color, const std::string &name)
		{
			auto &context = sparkle_test::OpenGLTestContext::instance();
			context.reset();
			context.setGeometry({.anchor = {0, 0}, .size = size});

			spk::ClearRenderCommand(color, spk::ClearRenderCommand::Mask::Color)
				.execute(context.renderContext());

			const std::filesystem::path category = "golden_image";
			const std::filesystem::path actual = sparkle_test::resultImagePath(category, name);
			const std::filesystem::path expected = sparkle_test::expectedImagePath(category, name);
			const std::filesystem::path difference =
				sparkle_test::resultImagePath(category, name + "_difference");

			context.save(actual);

			ASSERT_TRUE(std::filesystem::exists(expected))
				<< "Missing golden image: " << expected << "\n"
				<< "The generated candidate was saved to: " << actual;

			const sparkle_test::ImageComparisonResult result =
				sparkle_test::compareImages(actual, expected, difference);

			EXPECT_EQ(result.actualWidth, static_cast<int>(size.x));
			EXPECT_EQ(result.actualHeight, static_cast<int>(size.y));
			EXPECT_EQ(result.expectedWidth, static_cast<int>(size.x));
			EXPECT_EQ(result.expectedHeight, static_cast<int>(size.y));
			EXPECT_TRUE(result.matches)
				<< "Golden image mismatch for [" << name << "]\n"
				<< "Different pixels: " << result.differentPixelCount << "\n"
				<< "Actual image: " << actual << "\n"
				<< "Expected image: " << expected << "\n"
				<< "Difference image: " << difference;
		}
	};
}

TEST_F(ClientGoldenImageTest, Compares64By64Image)
{
	expectSolidFrame({64, 64}, {1.0f, 0.0f, 0.0f, 1.0f}, "solid_red_64x64");
}

TEST_F(ClientGoldenImageTest, Compares320By180Image)
{
	expectSolidFrame({320, 180}, {0.0f, 1.0f, 0.0f, 1.0f}, "solid_green_320x180");
}

TEST_F(ClientGoldenImageTest, Compares640By480Image)
{
	expectSolidFrame({640, 480}, {0.0f, 0.0f, 1.0f, 1.0f}, "solid_blue_640x480");
}
