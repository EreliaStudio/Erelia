#include "erelia/client/client_configuration.hpp"
#include "erelia/client/console.hpp"
#include "erelia/client/main_application_widget.hpp"
#include <core/application.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <gtest/gtest.h>
#include <type/uuid.hpp>

#include <chrono>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <string>

namespace
{
	class TemporaryJsonFile final
	{
	private:
		std::filesystem::path _path;

	public:
		explicit TemporaryJsonFile(const std::string &content)
		{
			_path = std::filesystem::temp_directory_path() /
					("erelia-client-" + spk::UUID::generate().toString() + ".json");
			std::ofstream stream(_path);
			stream << content;
		}

		~TemporaryJsonFile()
		{
			std::error_code error;
			std::filesystem::remove(_path, error);
		}

		[[nodiscard]] const std::filesystem::path &path() const noexcept
		{
			return _path;
		}
	};
}

TEST(ClientConfiguration, LoadsExactExternalContract)
{
	const TemporaryJsonFile file(
		R"({"server config":{"address":"127.0.0.1","port":2550,"retryDelayMs":15000}})");

	const ClientConfiguration configuration =
		ClientConfiguration::load(file.path());

	EXPECT_EQ(configuration.server.address, "127.0.0.1");
	EXPECT_EQ(configuration.server.port, 2550u);
	EXPECT_EQ(configuration.retryDelay, std::chrono::milliseconds(15000));
}

TEST(ClientConfiguration, RejectsInvalidContracts)
{
	const std::string fixtures[] = {
		R"({})",
		R"({"server config":{"address":"127.0.0.1","port":2550}})",
		R"({"server config":{"port":2550,"retryDelayMs":15000}})",
		R"({"server config":{"address":"127.0.0.1","retryDelayMs":15000}})",
		R"({"server config":{"address":"","port":2550,"retryDelayMs":15000}})",
		R"({"server config":{"address":"127.0.0.1","port":0,"retryDelayMs":15000}})",
		R"({"server config":{"address":"127.0.0.1","port":70000,"retryDelayMs":15000}})",
		R"({"server config":{"address":"127.0.0.1","port":2550,"retryDelayMs":0}})",
		R"({"server config":{"address":"127.0.0.1","port":2550,"retryDelayMs":-1}})",
		R"({"server config":{"address":"127.0.0.1","port":2550,"retryDelayMs":15000,"extra":true}})",
		R"({"server config":{"address":"127.0.0.1","port":2550,"retryDelayMs":15000},"extra":true})"};

	for (const std::string &fixture : fixtures)
	{
		const TemporaryJsonFile file(fixture);
		EXPECT_THROW(
			(void)ClientConfiguration::load(file.path()),
			spk::Exception);
	}
}

TEST(ClientConfiguration, ValidationExceptionRemainsStable)
{
	const TemporaryJsonFile file(
		R"({"server config":{"address":"","port":2550,"retryDelayMs":15000}})");

	try
	{
		(void)ClientConfiguration::load(file.path());
		FAIL() << "Expected spk::Exception";
	}
	catch (const spk::Exception &exception)
	{
		EXPECT_STREQ(
			exception.what(),
			"Client Server address cannot be empty");
	}
}

TEST(ClientConsole, OrdinarySubmissionUsesUserValueA)
{
	spk::Logger::Level receivedLevel = spk::Logger::Level::Trace;
	std::string receivedMessage;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &level, const std::string &message) {
			receivedLevel = level;
			receivedMessage = message;
		});

	SPK_LOG(UserValueA) << "player input" << std::endl;

	EXPECT_EQ(receivedLevel, spk::Logger::Level::UserValueA);
	EXPECT_EQ(receivedMessage, "player input");
}

TEST(ClientBootstrap, InitializesWidgetHierarchyFromConfiguredWindowGeometry)
{
	spk::Application application;
	spk::Window &mainWindow = application.createWindow(
		"main",
		spk::Window::Configuration{
			.title = "Erelia",
			.area = spk::Rect2D{
				.anchor = {0, 0},
				.size = {640, 480}}});
	MainApplicationWidget mainWidget(
		{"127.0.0.1", 1},
		std::chrono::milliseconds(1),
		&mainWindow.root());
	mainWidget.setGeometry(mainWindow.root().geometry());

	const spk::Rect2D expectedGeometry{
		.anchor = {0, 0},
		.size = {640, 480}};

	EXPECT_EQ(mainWindow.root().geometry(), expectedGeometry);
	EXPECT_EQ(mainWidget.geometry(), expectedGeometry);
	EXPECT_EQ(mainWidget.name(), "/MainApplicationWidget");
	EXPECT_EQ(
		mainWidget.connectionManager().name(),
		"/MainApplicationWidget/ConnectionManager");
	EXPECT_EQ(
		mainWidget.console().name(),
		"/MainApplicationWidget/Console");
	EXPECT_EQ(
		mainWidget.console().entryView().name(),
		"/MainApplicationWidget/Console/entries");
	EXPECT_EQ(
		mainWidget.console().commandEntry().name(),
		"/MainApplicationWidget/Console/command");
	EXPECT_EQ(mainWidget.console().geometry(), expectedGeometry);
	EXPECT_EQ(mainWidget.console().entryView().geometry().width, 640u);
	EXPECT_GT(mainWidget.console().entryView().geometry().height, 0u);
}
