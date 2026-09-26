#include "erelia/client/application.hpp"

#include "erelia/client/client_runtime.hpp"

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

int runClient(int argc, char **argv)
{
	try
	{
		spk::ArgumentParser arguments;
		arguments.setSynopsis(
			"EreliaClient --config <path>");
		arguments.addOption(
			{"config", 'c', "Path to the Client JSON configuration", 1});
		arguments.addOption(
			{"help", 'h', "Print this help"});
		arguments.parse(argc, argv);

		if (arguments.has("help") == true)
		{
			arguments.printHelp();
			return EXIT_SUCCESS;
		}

		if (arguments.has("config") == false)
		{
			throw spk::Exception(
				"Missing required option --config");
		}

		if (
			spk::Singleton<spk::WorkerPool>::isInstanciated() ==
			false)
		{
			spk::Singleton<spk::WorkerPool>::instanciate(
				new spk::WorkerPool());
		}

		ClientRuntime client(
			ClientRuntime::Configuration::load(
				arguments.get("config").values.front()));

		Running = 1;
		std::signal(SIGINT, onSignal);
		std::signal(SIGTERM, onSignal);

		ClientRuntime::ConnectionAnswer connection =
			client.connect();

		while (
			Running != 0 &&
			connection.status() ==
				ClientRuntime::ConnectionTask::Status::Pending)
		{
			std::this_thread::sleep_for(
				std::chrono::milliseconds(1));
		}

		if (Running == 0)
		{
			client.disconnect();
			return EXIT_SUCCESS;
		}

		try
		{
			(void)connection.get();
		} catch (...)
		{
			client.disconnect();
			throw;
		}

		if (client.isConnected() == false)
		{
			throw spk::Exception(
				"Client connection completed without an active Server connection");
		}

		SPK_LOG(Info)
			<< "Connected to dedicated Server"
			<< std::endl;

		while (
			Running != 0 &&
			client.isConnected() == true)
		{
			std::this_thread::sleep_for(
				std::chrono::milliseconds(1));
		}

		const bool localShutdown = Running == 0;
		client.disconnect();

		if (localShutdown == true)
		{
			return EXIT_SUCCESS;
		}

		throw spk::Exception(
			"Dedicated Server connection was lost");
	} catch (const std::exception &exception)
	{
		SPK_LOG(Error)
			<< exception.what()
			<< std::endl;
		return EXIT_FAILURE;
	}
}
