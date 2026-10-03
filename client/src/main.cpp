#include "erelia/client/client_configuration.hpp"
#include "erelia/client/client_runtime.hpp"
#include "erelia/client/command/connect_command.hpp"
#include "erelia/client/main_interface.hpp"
#include "erelia/client/service.hpp"
#include "erelia/client/world_manager.hpp"

#include <core/application.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <system/argument_parser.hpp>
#include <system/translator.hpp>

#include <cstdlib>
#include <exception>
#include <filesystem>
#include <utility>

int main(int argc, char **argv)
{
	try
	{
		spk::ArgumentParser arguments;

		arguments.setSynopsis("EreliaClient --config <path>");
		arguments.addOption({"config", 'c', "Path to the Client JSON configuration", 1});
		arguments.addOption({"help", 'h', "Print this help"});
		arguments.parse(argc, argv);

		if (arguments.has("help") == true)
		{
			arguments.printHelp();
			return EXIT_SUCCESS;
		}
		if (arguments.has("config") == false)
		{
			throw spk::Exception("Missing required option --config");
		}

		spk::logger.setLevelIdentifier(spk::Logger::Level::UserValueA, "User message");
		spk::logger.setLevelIdentifier(spk::Logger::Level::UserValueB, "Command");
		Service::translator().append(
			std::filesystem::absolute(argv[0]).parent_path() / "i18n" / "en.json");

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

		ClientRuntime runtime(
			std::move(configuration.server),
			configuration.retryDelay,
			&mainWindow.root());

		World *clientWorld =
			Service::clientWorldService().world(
				WorldIdentifier{.name = "prototype"});

		WorldManager world(
			"/WorldManager",
			configuration.terrain,
			&mainWindow.root());

		MainInterface interface(
			"/MainInterface",
			&mainWindow.root());
		interface.console().commandParser().addCommand<ConnectCommand>();

		world.setGeometry(mainWindow.root().geometry());
		interface.setGeometry(mainWindow.root().geometry());

		// The prototype client has no authoritative world identifier yet.
		// Keep selection explicit through the Client world lifecycle event.
		Service::clientEventCenter().worldChanged().trigger(clientWorld);

		return application.run();
	} catch (const std::exception &exception)
	{
		SPK_LOG(Error) << exception.what() << std::endl;
		return EXIT_FAILURE;
	}
}
