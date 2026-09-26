#include "erelia/server/application.hpp"

#include "erelia/server/router.hpp"

#include <system/argument_parser.hpp>

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

int runServer(int argc, char **argv)
{
	try
	{
		spk::ArgumentParser arguments;
		arguments.setSynopsis(
			"EreliaServer --config <path>");
		arguments.addOption(
			{"config", 'c', "Path to the router JSON configuration", 1});
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

		Router router(
			Router::Configuration::load(
				arguments.get("config").values.front()));

		Running = 1;
		std::signal(SIGINT, onSignal);
		std::signal(SIGTERM, onSignal);

		router.start();
		while (Running != 0)
		{
			router.dispatch();
			std::this_thread::sleep_for(
				std::chrono::milliseconds(1));
		}
		router.stop();

		return EXIT_SUCCESS;
	} catch (const std::exception &exception)
	{
		SPK_LOG(Error)
			<< exception.what()
			<< std::endl;
		return EXIT_FAILURE;
	}
}
