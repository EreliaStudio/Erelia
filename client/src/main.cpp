#include "erelia/client/client_configuration.hpp"
#include "erelia/client/main_application_widget.hpp"
#include "erelia/client/service.hpp"

#include <core/application.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <system/argument_parser.hpp>
#include <system/translator.hpp>

#include <cstdlib>
#include <filesystem>
#include <exception>
#include <utility>

int main(int argc, char **argv)
{
	try
	{
		spk::ArgumentParser arguments;

		arguments.setSynopsis("EreliaClient --config <path>");
		arguments.addOption(
			{
				"config",
				'c',
				Service::translator()->translate(
					"client.cli.config.description"),
				1});
		arguments.addOption(
			{
				"help",
				'h',
				Service::translator()->translate(
					"client.cli.help.description")});
		arguments.parse(argc, argv);

		if (arguments.has("help") == true)
		{
			arguments.printHelp();
			return EXIT_SUCCESS;
		}
		if (arguments.has("config") == false)
		{
			throw spk::Exception(
				Service::translator()->translate(
					"client.cli.config.missing"));
		}

		spk::logger.setLevelIdentifier(spk::Logger::Level::UserValueA, "User message");
		spk::logger.setLevelIdentifier(spk::Logger::Level::UserValueB, "Command");

		spk::Application application;
		spk::Window &mainWindow = application.createWindow(
			"main",
			spk::Window::Configuration{
				.title = "Erelia",
				.area = spk::Rect2D{
					.anchor = {0, 0},
					.size = {640, 480}}});

		const ClientConfiguration configuration =
			ClientConfiguration::load(std::filesystem::path(arguments.get("config").values.front()));

		MainApplicationWidget mainWidget(
			std::move(configuration.server),
			configuration.retryDelay,
			&mainWindow.root());
		mainWidget.setGeometry(mainWindow.root().geometry());

		return application.run();
	}
	catch (const std::exception &exception)
	{
		SPK_LOG(Error) << exception.what() << std::endl;
		return EXIT_FAILURE;
	}
}
