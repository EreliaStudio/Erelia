#include "erelia/client/application.hpp"
#include "erelia/client/console.hpp"
#include "erelia/client/main_application_widget.hpp"

#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <gtest/gtest.h>
#include <type/uuid.hpp>

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
		R"({"server config":{"address":"127.0.0.1","port":2550}})");

	const ClientConfiguration configuration =
		ClientConfiguration::load(file.path().string());

	EXPECT_EQ(configuration.server.address, "127.0.0.1");
	EXPECT_EQ(configuration.server.port, 2550u);
}

TEST(ClientConfiguration, RejectsInvalidContracts)
{
	const std::string fixtures[] = {
		R"({})",
		R"({"server config":{"address":"127.0.0.1"}})",
		R"({"server config":{"port":2550}})",
		R"({"server config":{"address":"","port":2550}})",
		R"({"server config":{"address":"127.0.0.1","port":0}})",
		R"({"server config":{"address":"127.0.0.1","port":70000}})",
		R"({"server config":{"address":"127.0.0.1","port":2550,"extra":true}})",
		R"({"server config":{"address":"127.0.0.1","port":2550},"extra":true})"};

	for (const std::string &fixture : fixtures)
	{
		const TemporaryJsonFile file(fixture);
		EXPECT_THROW(
			(void)ClientConfiguration::load(file.path().string()),
			spk::Exception);
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


TEST(ClientApplication, InitializesWidgetHierarchyFromConfiguredWindowGeometry)
{
	EreliaClientApplication application({"127.0.0.1", 1});
	const spk::Rect2D expectedGeometry{
		.anchor = {0, 0},
		.size = {640, 480}};

	EXPECT_EQ(application.window("main").root().geometry(), expectedGeometry);
	EXPECT_EQ(application.mainWidget().geometry(), expectedGeometry);
	EXPECT_EQ(application.mainWidget().console().geometry(), expectedGeometry);
	EXPECT_EQ(application.mainWidget().console().entryView().geometry().width, 640u);
	EXPECT_GT(application.mainWidget().console().entryView().geometry().height, 0u);
}
