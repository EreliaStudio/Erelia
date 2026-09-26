#include "__NODE_SNAKE___node_application.hpp"

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
#include <utility>

__NODE_NAME__NodeApplication::__NODE_NAME__NodeApplication(
	__NODE_NAME__Node::Configuration configuration) :
	_node(std::move(configuration))
{
}

void __NODE_NAME__NodeApplication::_onSignal(int)
{
	_signalReceived = 1;
}

void __NODE_NAME__NodeApplication::run()
{
	_stopRequested.store(
		false,
		std::memory_order_release);
	_running.store(
		false,
		std::memory_order_release);
	_signalReceived = 0;

	const auto previousInterruptHandler =
		std::signal(SIGINT, _onSignal);
	const auto previousTerminationHandler =
		std::signal(SIGTERM, _onSignal);

	try
	{
		_node.start();
		_running.store(
			true,
			std::memory_order_release);

		while (
			_stopRequested.load(std::memory_order_acquire) == false &&
			_signalReceived == 0)
		{
			_node.dispatch();
			std::this_thread::sleep_for(
				std::chrono::milliseconds(1));
		}

		_running.store(
			false,
			std::memory_order_release);
		_node.stop();
	} catch (...)
	{
		_running.store(
			false,
			std::memory_order_release);
		std::signal(SIGINT, previousInterruptHandler);
		std::signal(SIGTERM, previousTerminationHandler);
		throw;
	}

	std::signal(SIGINT, previousInterruptHandler);
	std::signal(SIGTERM, previousTerminationHandler);
}

void __NODE_NAME__NodeApplication::stop() noexcept
{
	_stopRequested.store(
		true,
		std::memory_order_release);
}

bool __NODE_NAME__NodeApplication::isRunning() const noexcept
{
	return _running.load(std::memory_order_acquire);
}

int run__NODE_NAME__Node(int argc, char **argv)
{
	try
	{
		spk::ArgumentParser arguments;
		arguments.setSynopsis(
			"Erelia__NODE_NAME__Node --config <path>");
		arguments.addOption(
			{"config", 'c', "Path to the node JSON configuration", 1});
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

		__NODE_NAME__NodeApplication application(
			__NODE_NAME__Node::Configuration::load(
				arguments.get("config").values.front()));
		application.run();

		return EXIT_SUCCESS;
	} catch (const std::exception &exception)
	{
		SPK_LOG(Error)
			<< exception.what()
			<< std::endl;
		return EXIT_FAILURE;
	}
}
