#include "erelia/client/client_runtime.hpp"
#include "erelia/client/command/connect_command.hpp"
#include "erelia/client/console.hpp"
#include "erelia/client/main_interface.hpp"
#include "erelia/client/world_manager.hpp"

#include <core/application.hpp>
#include <core/context/update_context.hpp>
#include <diagnostics/logger.hpp>
#include <input/device_context.hpp>
#include <rendering/render_snapshot.hpp>
#include <sparkle_test.hpp>

#include <gtest/gtest.h>

#include <chrono>
#include <filesystem>
#include <string>

namespace
{
	void advance(spk::Widget &widget)
	{
		spk::UpdateContext context{.time = {}, .deltaTime = std::chrono::milliseconds(16)};
		spk::Keyboard keyboard;
		spk::Mouse mouse;
		spk::DeviceContext devices{.keyboard = keyboard, .mouse = mouse};
		widget.updateState(context);
		widget.updateState(context, devices);
	}

	class PreparedApplication final
	{
	public:
		spk::Application application;
		spk::Window &mainWindow;
		ClientRuntime runtime;
		WorldManager world;
		MainInterface mainInterface;

		PreparedApplication() :
			mainWindow(
				application.createWindow(
					"main",
					spk::Window::Configuration{
						.title = "Erelia",
						.area = spk::Rect2D{
							.anchor = {0, 0},
							.size = {640, 480}}})),
			runtime(
				{"127.0.0.1", 1},
				std::chrono::milliseconds(1),
				&mainWindow.root()),
			world(
				"/WorldManager",
				runtime.terrainCollections(),
				{1, 2},
				&mainWindow.root()),
			mainInterface(
				"/MainInterface",
				&mainWindow.root())
		{
			mainInterface.console().commandParser().addCommand<ConnectCommand>();
			world.setGeometry(mainWindow.root().geometry());
			mainInterface.setGeometry(mainWindow.root().geometry());
		}

		[[nodiscard]] spk::Widget &root()
		{
			return mainWindow.root();
		}
	};

	spk::Widget &prepareApplication(PreparedApplication &prepared)
	{
		spk::Widget &root = prepared.root();
		const spk::Rect2D expectedGeometry{
			.anchor = {0, 0},
			.size = {640, 480}};
		EXPECT_EQ(root.geometry(), expectedGeometry);
		EXPECT_EQ(prepared.world.geometry(), expectedGeometry);
		EXPECT_EQ(prepared.mainInterface.geometry(), expectedGeometry);
		EXPECT_EQ(prepared.mainInterface.console().geometry(), expectedGeometry);
		advance(root);
		return root;
	}

	void expectWidgetImage(
		spk::Widget &widget,
		const std::filesystem::path &category,
		const std::string &name)
	{
		auto &context = sparkle_test::OpenGLTestContext::instance();
		context.reset();
		context.setGeometry({.anchor = {0, 0}, .size = {640, 480}});

		spk::RenderSnapshot::Builder builder;
		widget.buildRenderSnapshot(builder);
		builder.build().execute(context.renderContext());

		const auto actual = sparkle_test::resultImagePath(category, name);
		const auto expected = sparkle_test::expectedImagePath(category, name);
		const auto difference = sparkle_test::resultImagePath(category, name + "_difference");
		context.save(actual);

		ASSERT_TRUE(std::filesystem::exists(expected))
			<< "Missing golden image: " << expected << "\n"
			<< "The generated candidate was saved to: " << actual;

		const auto result = sparkle_test::compareImages(actual, expected, difference);
		EXPECT_EQ(result.actualWidth, 640);
		EXPECT_EQ(result.actualHeight, 480);
		EXPECT_EQ(result.expectedWidth, 640);
		EXPECT_EQ(result.expectedHeight, 480);
		EXPECT_TRUE(result.matches)
			<< "Golden image mismatch for [" << category.string() << '/' << name << "]\n"
			<< "Different pixels: " << result.differentPixelCount << "\n"
			<< "Actual image: " << actual << "\n"
			<< "Expected image: " << expected << "\n"
			<< "Difference image: " << difference;
	}

	class ClientGoldenImageTest : public ::testing::Test
	{
	protected:
		static void SetUpTestSuite()
		{
			sparkle_test::configurePaths(
				std::filesystem::path{ERELIA_CLIENT_TEST_RESOURCES_DIR},
				std::filesystem::path{ERELIA_CLIENT_TEST_RESULTS_DIR});
		}
	};
}

TEST_F(ClientGoldenImageTest, ConsoleEmpty)
{
	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});
	expectWidgetImage(console, "console", "empty");
}

TEST_F(ClientGoldenImageTest, ConsoleUserAndCommandMessages)
{
	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});
	SPK_LOG(UserValueA) << "Hello from the player" << std::endl;
	SPK_LOG(UserValueB) << "Connection cycle started" << std::endl;
	advance(console);
	expectWidgetImage(console, "console", "user_and_command");
}

TEST_F(ClientGoldenImageTest, ConsoleAllLoggerLevels)
{
	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});
	SPK_LOG(Trace) << "Trace message" << std::endl;
	SPK_LOG(Info) << "Info message" << std::endl;
	SPK_LOG(UserValueA) << "User message" << std::endl;
	SPK_LOG(UserValueB) << "Command result" << std::endl;
	SPK_LOG(Warning) << "Warning message" << std::endl;
	SPK_LOG(Error) << "Error message" << std::endl;
	advance(console);
	expectWidgetImage(console, "console", "all_levels");
}

TEST_F(ClientGoldenImageTest, ConsoleOverflowTopMiddleAndBottom)
{
	Console console("Console");
	console.setGeometry({.anchor = {0, 0}, .size = {640, 480}});
	for (std::size_t index = 0; index < 200; ++index)
	{
		SPK_LOG(UserValueA) << "history-" << index << std::endl;
	}
	advance(console);

	console.entryView().scrollTo(0);
	expectWidgetImage(console, "console", "overflow_top");
	console.entryView().scrollTo(100);
	expectWidgetImage(console, "console", "overflow_middle");
	console.entryView().scrollTo(199);
	expectWidgetImage(console, "console", "overflow_bottom");
}

TEST_F(ClientGoldenImageTest, ApplicationStartup)
{
	PreparedApplication application;
	spk::Widget &root = prepareApplication(application);
	expectWidgetImage(root, "application", "startup");
}

TEST_F(ClientGoldenImageTest, ApplicationMixedConsole)
{
	PreparedApplication application;
	spk::Widget &root = prepareApplication(application);
	Console &console = application.mainInterface.console();
	SPK_LOG(UserValueA) << "Player message" << std::endl;
	SPK_LOG(UserValueB) << "Command result" << std::endl;
	SPK_LOG(Info) << "System information" << std::endl;
	SPK_LOG(Warning) << "System warning" << std::endl;
	advance(console);
	expectWidgetImage(root, "application", "mixed_console");
}

TEST_F(ClientGoldenImageTest, ApplicationConsoleOverflow)
{
	PreparedApplication application;
	spk::Widget &root = prepareApplication(application);
	Console &console = application.mainInterface.console();
	for (std::size_t index = 0; index < 200; ++index)
	{
		SPK_LOG(UserValueA) << "application-history-" << index << std::endl;
	}
	advance(console);
	ASSERT_TRUE(console.entryView().isLastRowVisible());
	expectWidgetImage(root, "application", "console_overflow");
}
