#include "terrain_node_application.hpp"

#include "erelia/core/chunk_protocol_error.hpp"
#include "erelia/core/chunk_protocol_request.hpp"
#include "erelia/core/networking/diagnostic.hpp"
#include "erelia/core/networking/message_type.hpp"

#include <system/argument_parser.hpp>

#include <design_pattern/singleton.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <threading/worker_pool.hpp>

#include <chrono>
#include <csignal>
#include <cstdlib>
#include <exception>
#include <set>
#include <string>
#include <string_view>
#include <thread>
#include <utility>
#include <vector>

namespace
{
	constexpr std::string_view DuplicateCoordinateKey =
		"Chunk_Coordinates_Duplication";
	constexpr std::string_view MalformedRequestKey =
		"Chunk_Request_Malformed";
}

TerrainNodeApplication::TerrainNodeApplication(
	TerrainNode::Configuration configuration) :
	_node(std::move(configuration))
{
}

void TerrainNodeApplication::_onSignal(int)
{
	_signalReceived = 1;
}

void TerrainNodeApplication::_treatMessages()
{
	for (TerrainNode::Request &request :
		 _node.requests().drain(_requests))
	{
		_treatMessage(std::move(request));
	}
}

void TerrainNodeApplication::_treatMessage(
	TerrainNode::Request request)
{
	switch (static_cast<Networking::MessageType>(
		request.message.type()))
	{
	case Networking::MessageType::ChunkRequest:
		_parseChunkRequest(std::move(request));
		break;

	default:
		SPK_LOG(Warning)
			<< "TerrainNode received unsupported message type "
			<< request.message.type()
			<< std::endl;
		break;
	}
}

void TerrainNodeApplication::_parseChunkRequest(
	TerrainNode::Request request)
{
	try
	{
		const Chunk::Protocol::Request chunkRequest(
			request.message);
		const std::set<Chunk::Coordinate> duplicates =
			chunkRequest.duplicateCoordinates();

		if (!duplicates.empty())
		{
			Chunk::Protocol::Error::Builder builder(
				chunkRequest.requestID(),
				Networking::Diagnostic::Severity::Warning,
				std::string(DuplicateCoordinateKey));

			for (const Chunk::Coordinate &coordinate :
				 duplicates)
			{
				builder.add(coordinate);
			}

			_node.reply(
				request,
				std::move(builder).build());
		}

		std::set<Chunk::Coordinate> seen;
		std::vector<Chunk::Coordinate> coordinates;
		coordinates.reserve(
			chunkRequest.coordinateCount());

		for (
			std::size_t index = 0u;
			index < chunkRequest.coordinateCount();
			++index)
		{
			const Chunk::Coordinate coordinate =
				chunkRequest.coordinate(index);
			if (seen.insert(coordinate).second)
			{
				coordinates.push_back(coordinate);
			}
		}

		_node.requestChunks(
			std::move(request),
			std::move(coordinates));
	} catch (const spk::Exception &exception)
	{
		SPK_LOG(Error)
			<< "Malformed Chunk request: "
			<< exception.what()
			<< std::endl;

		Networking::Diagnostic::Builder builder(
			Networking::Diagnostic::Severity::Error,
			std::string(MalformedRequestKey),
			request.message.requestID());
		_node.reply(
			request,
			std::move(builder).build());
	}
}

void TerrainNodeApplication::run()
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
			_treatMessages();
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

void TerrainNodeApplication::stop() noexcept
{
	_stopRequested.store(
		true,
		std::memory_order_release);
}

bool TerrainNodeApplication::isRunning() const noexcept
{
	return _running.load(std::memory_order_acquire);
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

		TerrainNodeApplication application(
			TerrainNode::Configuration::load(
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
