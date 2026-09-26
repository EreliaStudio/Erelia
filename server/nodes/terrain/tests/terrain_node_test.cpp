#include "terrain_node.hpp"
#include "terrain_node_application.hpp"

#include "erelia/core/chunk_protocol_error.hpp"
#include "erelia/core/chunk_protocol_request.hpp"
#include "erelia/core/chunk_protocol_response.hpp"
#include "erelia/core/networking/diagnostic.hpp"
#include "erelia/core/networking/message_type.hpp"
#include "erelia/server/router.hpp"

#include <design_pattern/singleton.hpp>
#include <exception.hpp>
#include <gtest/gtest.h>
#include <network/client.hpp>
#include <network/remote_node.hpp>
#include <threading/worker_pool.hpp>
#include <type/uuid.hpp>

#include <algorithm>
#include <chrono>
#include <csignal>
#include <cstdint>
#include <exception>
#include <filesystem>
#include <fstream>
#include <string>
#include <thread>
#include <utility>
#include <vector>

using namespace std::chrono_literals;

namespace
{
	class TemporaryJsonFile
	{
	private:
		std::filesystem::path _path;

	public:
		explicit TemporaryJsonFile(
			const std::string &content)
		{
			_path =
				std::filesystem::temp_directory_path() /
				("erelia-terrain-node-" +
				 spk::UUID::generate().toString() +
				 ".json");
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

	template <typename TPredicate>
	[[nodiscard]] bool waitUntil(
		TPredicate predicate,
		std::chrono::milliseconds timeout = 2s)
	{
		const auto deadline =
			std::chrono::steady_clock::now() + timeout;
		while (std::chrono::steady_clock::now() < deadline)
		{
			if (predicate())
			{
				return true;
			}
			std::this_thread::sleep_for(5ms);
		}
		return predicate();
	}

	void expectApplicationStopsOnSignal(int signal)
	{
		TerrainNodeApplication application(
			TerrainNode::Configuration{
				.port = 0});

		std::exception_ptr applicationFailure;
		std::thread applicationThread(
			[&]() {
				try
				{
					application.run();
				} catch (...)
				{
					applicationFailure =
						std::current_exception();
				}
			});

		if (!waitUntil(
				[&]() {
					return application.isRunning();
				}))
		{
			application.stop();
			applicationThread.join();
			ADD_FAILURE()
				<< "Terrain node application did not start before the deadline";
			return;
		}

		if (std::raise(signal) != 0)
		{
			application.stop();
			applicationThread.join();
			ADD_FAILURE()
				<< "Unable to raise signal "
				<< signal;
			return;
		}

		if (!waitUntil(
				[&]() {
					return application.isRunning() == false;
				}))
		{
			application.stop();
			applicationThread.join();
			ADD_FAILURE()
				<< "Terrain node application did not stop after signal "
				<< signal;
			return;
		}

		applicationThread.join();
		EXPECT_FALSE(applicationFailure);
	}

	void ensureWorkerPool()
	{
		if (!spk::Singleton<spk::WorkerPool>::isInstanciated())
		{
			spk::Singleton<spk::WorkerPool>::instanciate(
				new spk::WorkerPool());
		}
	}

	[[nodiscard]] std::uint16_t availablePort()
	{
		spk::RemoteNode::Endpoint probe;
		probe.start(0);
		const std::uint16_t port = probe.port();
		probe.stop();
		return port;
	}

	class TerrainApplicationRunner final
	{
	private:
		TerrainNodeApplication _application;
		std::thread _thread;
		std::exception_ptr _failure;

	public:
		explicit TerrainApplicationRunner(
			std::uint16_t port) :
			_application(
				TerrainNode::Configuration{
					.port = port}),
			_thread(
				[this] {
					try
					{
						_application.run();
					}
					catch (...)
					{
						_failure =
							std::current_exception();
					}
				})
		{
		}

		~TerrainApplicationRunner()
		{
			_application.stop();
			if (_thread.joinable())
			{
				_thread.join();
			}
		}

		[[nodiscard]] bool waitUntilRunning()
		{
			return waitUntil(
				[this] {
					return _application.isRunning();
				});
		}

		[[nodiscard]] std::exception_ptr failure() const
		{
			return _failure;
		}
	};

	void configureChunkRouting(Router &router)
	{
		router.redirect(
			static_cast<spk::Message::Type>(
				Networking::MessageType::ChunkRequest),
			"terrain");
	}

	[[nodiscard]] bool waitUntilTerrainConnected(
		Router &router)
	{
		return waitUntil(
			[&] {
				router.dispatch();
				return router.isNodeConnected("terrain");
			});
	}

	[[nodiscard]] bool collectMessages(
		Router &router,
		spk::Client &client,
		std::vector<spk::Message> &destination,
		std::size_t expectedCount)
	{
		std::vector<spk::Message> drained;
		return waitUntil(
			[&] {
				router.dispatch();
				for (spk::Message &message :
					 client.messages().drain(drained))
				{
					destination.push_back(
						std::move(message));
				}
				return destination.size() >= expectedCount;
			});
	}
}

TEST(TerrainNodeConfiguration, LoadsAndRejectsExactContract)
{
	const TemporaryJsonFile valid(
		R"({"server config":{"port":0}})");
	EXPECT_EQ(
		TerrainNode::Configuration::load(
			valid.path().string())
			.port,
		0u);

	const std::string invalidFixtures[] = {
		R"({})",
		R"({"server config":{}})",
		R"({"server config":{"port":70000}})",
		R"({"server config":{"port":0,"extra":true}})",
		R"({"server config":{"port":0},"extra":true})"};

	for (const std::string &fixture : invalidFixtures)
	{
		const TemporaryJsonFile file(fixture);
		EXPECT_THROW(
			(void)TerrainNode::Configuration::load(
				file.path().string()),
			spk::Exception);
	}
}

TEST(TerrainNodeRuntime, StartsStopsAndRestarts)
{
	TerrainNode node(
		TerrainNode::Configuration{
			.port = 0});

	node.start();
	EXPECT_TRUE(node.isRunning());
	EXPECT_NE(node.port(), 0u);

	node.stop();
	EXPECT_FALSE(node.isRunning());
	EXPECT_EQ(node.port(), 0u);

	node.start();
	EXPECT_TRUE(node.isRunning());
	EXPECT_NE(node.port(), 0u);
	node.stop();
}

TEST(TerrainNodeApplication, StopsCleanlyOnInterruptSignal)
{
	expectApplicationStopsOnSignal(SIGINT);
}

TEST(TerrainNodeApplication, StopsCleanlyOnTerminationSignal)
{
	expectApplicationStopsOnSignal(SIGTERM);
}


TEST(TerrainNodeIntegration, ReturnsOneResponseForMultiCoordinateRequest)
{
	ensureWorkerPool();
	const std::uint16_t terrainPort = availablePort();
	TerrainApplicationRunner application(terrainPort);
	ASSERT_TRUE(application.waitUntilRunning());

	Router router(
		Router::Configuration{
			.port = 0,
			.nodeReconnectDelay = 10ms,
			.nodes = {
				{"terrain", "127.0.0.1", terrainPort}}});
	configureChunkRouting(router);
	router.start();
	ASSERT_TRUE(waitUntilTerrainConnected(router));

	spk::Client client;
	client.connect("127.0.0.1", router.port());

	Chunk::Protocol::Request::Builder builder;
	builder.add({-2, 3, 4});
	builder.add({5, 0, -1});
	const auto request = std::move(builder).build();
	client.send(request);

	std::vector<spk::Message> messages;
	ASSERT_TRUE(
		collectMessages(
			router,
			client,
			messages,
			1u));
	ASSERT_EQ(messages.size(), 1u);

	const Chunk::Protocol::Response response(
		messages.front());
	EXPECT_EQ(
		response.requestID(),
		request.requestID());
	EXPECT_EQ(response.successCount(), 2u);
	EXPECT_EQ(response.failureCount(), 0u);
	EXPECT_EQ(
		response.success(0u).coordinate,
		(Chunk::Coordinate{-2, 3, 4}));
	EXPECT_EQ(
		response.success(1u).coordinate,
		(Chunk::Coordinate{5, 0, -1}));

	client.disconnect();
	router.stop();
	EXPECT_FALSE(application.failure());
}

TEST(TerrainNodeIntegration, DiagnosesDuplicatesAndProcessesFirstOccurrence)
{
	ensureWorkerPool();
	const std::uint16_t terrainPort = availablePort();
	TerrainApplicationRunner application(terrainPort);
	ASSERT_TRUE(application.waitUntilRunning());

	Router router(
		Router::Configuration{
			.port = 0,
			.nodeReconnectDelay = 10ms,
			.nodes = {
				{"terrain", "127.0.0.1", terrainPort}}});
	configureChunkRouting(router);
	router.start();
	ASSERT_TRUE(waitUntilTerrainConnected(router));

	spk::Client client;
	client.connect("127.0.0.1", router.port());

	const Chunk::Coordinate coordinate{-3, 7, 1};
	spk::Message request(
		static_cast<spk::Message::Type>(
			Networking::MessageType::ChunkRequest));
	request.setRequestID(700u);
	request << coordinate;
	request << coordinate;
	client.send(request);

	std::vector<spk::Message> messages;
	ASSERT_TRUE(
		collectMessages(
			router,
			client,
			messages,
			2u));
	ASSERT_EQ(messages.size(), 2u);
	EXPECT_EQ(
		messages[0].type(),
		static_cast<spk::Message::Type>(
			Networking::MessageType::ChunkError));

	const Chunk::Protocol::Error error(
		messages[0]);
	EXPECT_EQ(error.requestID(), 700u);
	EXPECT_EQ(
		error.severity(),
		Networking::Diagnostic::Severity::Warning);
	EXPECT_EQ(
		error.message(),
		"Chunk_Coordinates_Duplication");
	ASSERT_EQ(error.coordinateCount(), 1u);
	EXPECT_EQ(error.coordinate(0u), coordinate);

	const Chunk::Protocol::Response response(
		messages[1]);
	EXPECT_EQ(response.requestID(), 700u);
	ASSERT_EQ(response.successCount(), 1u);
	EXPECT_EQ(response.failureCount(), 0u);
	EXPECT_EQ(
		response.success(0u).coordinate,
		coordinate);

	client.disconnect();
	router.stop();
	EXPECT_FALSE(application.failure());
}

TEST(TerrainNodeIntegration, MalformedRequestReturnsDiagnosticWithoutChunkResponse)
{
	ensureWorkerPool();
	const std::uint16_t terrainPort = availablePort();
	TerrainApplicationRunner application(terrainPort);
	ASSERT_TRUE(application.waitUntilRunning());

	Router router(
		Router::Configuration{
			.port = 0,
			.nodeReconnectDelay = 10ms,
			.nodes = {
				{"terrain", "127.0.0.1", terrainPort}}});
	configureChunkRouting(router);
	router.start();
	ASSERT_TRUE(waitUntilTerrainConnected(router));

	spk::Client client;
	client.connect("127.0.0.1", router.port());

	spk::Message request(
		static_cast<spk::Message::Type>(
			Networking::MessageType::ChunkRequest));
	request.setRequestID(701u);
	request << std::uint8_t{42u};
	client.send(request);

	std::vector<spk::Message> messages;
	ASSERT_TRUE(
		collectMessages(
			router,
			client,
			messages,
			1u));
	ASSERT_EQ(messages.size(), 1u);

	const Networking::Diagnostic diagnostic(
		messages.front());
	EXPECT_EQ(diagnostic.requestID(), 701u);
	EXPECT_EQ(
		diagnostic.severity(),
		Networking::Diagnostic::Severity::Error);
	EXPECT_EQ(
		diagnostic.message(),
		"Chunk_Request_Malformed");

	std::vector<spk::Message> additional;
	const bool receivedAdditional =
		waitUntil(
			[&] {
				router.dispatch();
				client.messages().drain(additional);
				return !additional.empty();
			},
			100ms);
	EXPECT_FALSE(receivedAdditional);

	client.disconnect();
	router.stop();
	EXPECT_FALSE(application.failure());
}
