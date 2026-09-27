#include "erelia/client/application.hpp"

#include "erelia/client/main_application_widget.hpp"

#include <container/json/reader.hpp>
#include <design_pattern/singleton.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <network/client.hpp>
#include <system/argument_parser.hpp>
#include <threading/worker_pool.hpp>

#include <cstdlib>
#include <filesystem>
#include <utility>

namespace
{
	constexpr char WindowIdentifier[] = "main";
}

ClientConfiguration ClientConfiguration::load(const std::string &path)
{
	const std::filesystem::path file(path);
	const spk::JSON::Value document = spk::JSON::Loader::parseFile(file);
	const spk::JSON::Reader root(document, file);
	root.forbidUnknown({"server config"});

	const spk::JSON::Reader server = root.child("server config");
	server.forbidUnknown({"address", "port"});

	ClientConfiguration result{
		.server = {
			.address = server.require<std::string>("address"),
			.port = server.require<std::uint16_t>("port")}};

	if (result.server.address.empty() == true)
	{
		throw spk::Exception("Client Server address cannot be empty");
	}
	if (result.server.port == 0)
	{
		throw spk::Exception("Client Server port cannot be zero");
	}
	return result;
}

EreliaClientApplication::EreliaClientApplication(
	ConnectionManager::Endpoint endpoint)
{
	if (spk::Singleton<spk::WorkerPool>::isInstanciated() == false)
	{
		spk::Singleton<spk::WorkerPool>::instanciate(new spk::WorkerPool());
	}
	if (spk::Singleton<spk::Client>::isInstanciated() == false)
	{
		spk::Singleton<spk::Client>::instanciate(new spk::Client());
	}

	spk::logger.setLevelIdentifier(spk::Logger::Level::UserValueA, "User message");
	spk::logger.setLevelIdentifier(spk::Logger::Level::UserValueB, "Command");

	spk::Window &mainWindow = createWindow(
		WindowIdentifier,
		spk::Window::Configuration{
			.title = "Erelia",
			.area = {{0, 0}, {640, 480}}});

	_mainWidget = std::make_unique<MainApplicationWidget>(
		std::move(endpoint),
		&mainWindow.root());
	_mainWidget->setGeometry(mainWindow.geometry());
}

EreliaClientApplication::~EreliaClientApplication() = default;

int runClient(int argc, char **argv)
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

		const ClientConfiguration configuration =
			ClientConfiguration::load(arguments.get("config").values.front());
		EreliaClientApplication application(configuration.server);
		return application.run();
	} catch (const std::exception &exception)
	{
		SPK_LOG(Error) << exception.what() << std::endl;
		return EXIT_FAILURE;
	}
}
