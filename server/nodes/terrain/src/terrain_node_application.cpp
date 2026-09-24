#include "terrain_node_application.hpp"

#include "terrain_node.hpp"

#include <structure/system/argument_parser.hpp>

#include <design_pattern/singleton.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <threading/worker_pool.hpp>

#include <chrono>
#include <csignal>
#include <cstdlib>
#include <exception>
#include <thread>

namespace
{
	volatile std::sig_atomic_t Running = 1;

	void onSignal(int)
	{
		Running = 0;
	}
}

int runTerrainNode(int argc, char **argv)
{
	try
	{
		spk::ArgumentParser arguments;
		arguments.setSynopsis(
			"EreliaTerrainNode --config <path>");
		arguments.addOption(
			{"config", 'c', "Path to the terrain node JSON configuration", 1});
		arguments.addOption(
			{"help", 'h', "Print this help"});
		arguments.parse(argc, argv);

		if (arguments.has("help"))
		{
			arguments.printHelp();
			return EXIT_SUCCESS;
		}

		if (!arguments.has("config"))
		{
			throw spk::Exception(
				"Missing required option --config");
		}

		spk::Singleton<spk::WorkerPool>::instanciate(
			new spk::WorkerPool());

		TerrainNode node(
			TerrainNode::Configuration::load(
				arguments.get("config").values.front()));

		Running = 1;
		std::signal(SIGINT, onSignal);
		std::signal(SIGTERM, onSignal);

		node.start();
		while (Running != 0)
		{
			node.dispatch();
			std::this_thread::sleep_for(
				std::chrono::milliseconds(1));
		}
		node.stop();

		return EXIT_SUCCESS;
	}
	catch (const std::exception &exception)
	{
		SPK_LOG(Error)
			<< exception.what()
			<< std::endl;
		return EXIT_FAILURE;
	}
}
