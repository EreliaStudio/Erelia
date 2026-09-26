#include "terrain_node_application.hpp"

#include "erelia/core/chunk_protocol_error.hpp"
#include "erelia/core/chunk_protocol_request.hpp"
#include "erelia/core/chunk_protocol_response.hpp"
#include "erelia/core/networking/diagnostic.hpp"
#include "erelia/core/networking/message_type.hpp"
#include "erelia/server/router.hpp"

#include <design_pattern/singleton.hpp>
#include <gtest/gtest.h>
#include <network/client.hpp>
#include <network/remote_node.hpp>
#include <threading/worker_pool.hpp>

#include <atomic>
#include <chrono>
#include <cstddef>
#include <cstdint>
#include <memory>
#include <thread>
#include <utility>
#include <vector>

using namespace std::chrono_literals;

namespace
{
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

	void ensureWorkerPool()
	{
		if (!spk::Singleton<spk::WorkerPool>::isInstanciated())
		{
			spk::Singleton<spk::WorkerPool>::instanciate(
				new spk::WorkerPool());
		}
	}

	class WorkerPoolBlocker final
	{
	private:
		std::shared_ptr<std::atomic_bool> _release =
			std::make_shared<std::atomic_bool>(false);
		std::shared_ptr<std::atomic<std::size_t>> _started =
			std::make_shared<std::atomic<std::size_t>>(0u);
		std::size_t _workerCount = 0u;
		bool _released = false;

	public:
		WorkerPoolBlocker()
		{
			spk::WorkerPool &workerPool =
				spk::Singleton<spk::WorkerPool>::instance();
			_workerCount = workerPool.workerCount();

			for (std::size_t index = 0u;
				 index < _workerCount;
				 ++index)
			{
				(void)workerPool.submit(
					[release = _release,
					 started = _started] {
						started->fetch_add(
							1u,
							std::memory_order_release);
						release->wait(
							false,
							std::memory_order_acquire);
						return true;
					});
			}
		}

		~WorkerPoolBlocker()
		{
			release();
		}

		[[nodiscard]] bool waitUntilBlocked() const
		{
			return waitUntil(
				[this] {
					return _started->load(
							std::memory_order_acquire) ==
						_workerCount;
				});
		}

		void release()
		{
			if (_released)
			{
				return;
			}

			_released = true;
			_release->store(
				true,
				std::memory_order_release);
			_release->notify_all();
		}
	};

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
		std::atomic_bool _failed{false};

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
					} catch (...)
					{
						_failed.store(
							true,
							std::memory_order_release);
					}
				})
		{
		}

		~TerrainApplicationRunner()
		{
			stop();
		}

		void stop()
		{
			_application.stop();
			if (_thread.joinable())
			{
				_thread.join();
			}
		}

		[[nodiscard]] bool failed() const noexcept
		{
			return _failed.load(
				std::memory_order_acquire);
		}

		[[nodiscard]] bool waitUntilRunning()
		{
			return waitUntil(
				[this] {
					return _application.isRunning();
				});
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

TEST(ClientServerIntegration, ChunkRequestReturnsCanonicalTerrainChunk)
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

	const Chunk::Coordinate coordinate{1, 0, 1};
	Chunk::Protocol::Request::Builder builder;
	builder.add(coordinate);
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
	EXPECT_EQ(response.requestID(), request.requestID());
	ASSERT_EQ(response.successCount(), 1u);
	EXPECT_EQ(response.failureCount(), 0u);

	const auto success = response.success(0u);
	EXPECT_EQ(success.coordinate, coordinate);

	const Voxel::Cell baseline =
		success.chunk.at({3, 0, 8});
	EXPECT_EQ(baseline.definitionId(), 1u);
	EXPECT_EQ(
		baseline.orientation(),
		Voxel::Cell::Orientation::PositiveX);
	EXPECT_EQ(
		baseline.flipOrientation(),
		Voxel::Cell::FlipOrientation::PositiveY);

	const Voxel::Cell slope =
		success.chunk.at({4, 1, 4});
	EXPECT_EQ(slope.definitionId(), 2u);
	EXPECT_EQ(
		slope.orientation(),
		Voxel::Cell::Orientation::PositiveX);
	EXPECT_EQ(
		slope.flipOrientation(),
		Voxel::Cell::FlipOrientation::PositiveY);

	EXPECT_EQ(
		success.chunk.at({3, 1, 8}).packed(),
		Voxel::Cell::Empty.packed());

	client.disconnect();
	router.stop();
	application.stop();
	EXPECT_FALSE(application.failed());
}

TEST(ClientServerIntegration, MultiCoordinateRequestReturnsEveryCanonicalChunk)
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

	const Chunk::Coordinate slopeCoordinate{1, 0, 1};
	const Chunk::Coordinate alternateSlopeCoordinate{2, 0, 1};
	const Chunk::Coordinate negativeCoordinate{-1, 0, 0};

	Chunk::Protocol::Request::Builder builder;
	builder.add(alternateSlopeCoordinate);
	builder.add(negativeCoordinate);
	builder.add(slopeCoordinate);
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
	EXPECT_EQ(response.requestID(), request.requestID());
	ASSERT_EQ(response.successCount(), 3u);
	EXPECT_EQ(response.failureCount(), 0u);

	bool sawSlope = false;
	bool sawAlternateSlope = false;
	bool sawNegative = false;

	for (std::size_t index = 0u;
		 index < response.successCount();
		 ++index)
	{
		const auto success = response.success(index);

		if (success.coordinate == slopeCoordinate)
		{
			sawSlope = true;
			EXPECT_EQ(
				success.chunk.at({4, 1, 4}).definitionId(),
				2u);
		}
		else if (success.coordinate == alternateSlopeCoordinate)
		{
			sawAlternateSlope = true;
			EXPECT_EQ(
				success.chunk.at({4, 1, 4}).definitionId(),
				3u);
		}
		else if (success.coordinate == negativeCoordinate)
		{
			sawNegative = true;
			EXPECT_EQ(
				success.chunk.at({3, 0, 8}).definitionId(),
				1u);
		}
		else
		{
			ADD_FAILURE()
				<< "Unexpected Chunk coordinate in multi-coordinate response";
		}
	}

	EXPECT_TRUE(sawSlope);
	EXPECT_TRUE(sawAlternateSlope);
	EXPECT_TRUE(sawNegative);

	client.disconnect();
	router.stop();
	application.stop();
	EXPECT_FALSE(application.failed());
}

TEST(ClientServerIntegration, ConcurrentClientsReceiveOnlyTheirOwnChunkResponses)
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

	spk::Client firstClient;
	spk::Client secondClient;
	firstClient.connect("127.0.0.1", router.port());
	secondClient.connect("127.0.0.1", router.port());

	const Chunk::Coordinate firstCoordinate{1, 0, 1};
	const Chunk::Coordinate secondCoordinate{1, 0, 2};

	Chunk::Protocol::Request::Builder firstBuilder;
	firstBuilder.add(firstCoordinate);
	const auto firstRequest =
		std::move(firstBuilder).build();

	Chunk::Protocol::Request::Builder secondBuilder;
	secondBuilder.add(secondCoordinate);
	const auto secondRequest =
		std::move(secondBuilder).build();

	ASSERT_NE(
		firstRequest.requestID(),
		secondRequest.requestID());

	firstClient.send(firstRequest);
	secondClient.send(secondRequest);

	std::vector<spk::Message> firstMessages;
	std::vector<spk::Message> secondMessages;
	ASSERT_TRUE(
		collectMessages(
			router,
			firstClient,
			firstMessages,
			1u));
	ASSERT_TRUE(
		collectMessages(
			router,
			secondClient,
			secondMessages,
			1u));
	ASSERT_EQ(firstMessages.size(), 1u);
	ASSERT_EQ(secondMessages.size(), 1u);

	const Chunk::Protocol::Response firstResponse(
		firstMessages.front());
	const Chunk::Protocol::Response secondResponse(
		secondMessages.front());

	EXPECT_EQ(
		firstResponse.requestID(),
		firstRequest.requestID());
	EXPECT_EQ(
		secondResponse.requestID(),
		secondRequest.requestID());

	ASSERT_EQ(firstResponse.successCount(), 1u);
	ASSERT_EQ(secondResponse.successCount(), 1u);
	EXPECT_EQ(firstResponse.failureCount(), 0u);
	EXPECT_EQ(secondResponse.failureCount(), 0u);

	const auto firstSuccess = firstResponse.success(0u);
	const auto secondSuccess = secondResponse.success(0u);
	EXPECT_EQ(firstSuccess.coordinate, firstCoordinate);
	EXPECT_EQ(secondSuccess.coordinate, secondCoordinate);
	EXPECT_EQ(
		firstSuccess.chunk.at({4, 1, 4}).definitionId(),
		2u);
	EXPECT_EQ(
		secondSuccess.chunk.at({4, 1, 4}).definitionId(),
		4u);

	firstClient.disconnect();
	secondClient.disconnect();
	router.stop();
	application.stop();
	EXPECT_FALSE(application.failed());
}

TEST(ClientServerIntegration, DisconnectDuringOutstandingRequestKeepsTerrainOperational)
{
	ensureWorkerPool();
	WorkerPoolBlocker workerPoolBlocker;
	ASSERT_TRUE(workerPoolBlocker.waitUntilBlocked());

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

	spk::Client disconnectedClient;
	disconnectedClient.connect("127.0.0.1", router.port());

	const Chunk::Coordinate outstandingCoordinate{-3, 0, 1};
	spk::Message outstandingRequest(
		static_cast<spk::Message::Type>(
			Networking::MessageType::ChunkRequest));
	outstandingRequest.setRequestID(702u);
	outstandingRequest << outstandingCoordinate;
	outstandingRequest << outstandingCoordinate;
	disconnectedClient.send(outstandingRequest);

	std::vector<spk::Message> diagnosticMessages;
	ASSERT_TRUE(
		collectMessages(
			router,
			disconnectedClient,
			diagnosticMessages,
			1u));
	ASSERT_EQ(diagnosticMessages.size(), 1u);

	const Chunk::Protocol::Error diagnostic(
		diagnosticMessages.front());
	EXPECT_EQ(diagnostic.requestID(), 702u);
	EXPECT_EQ(
		diagnostic.message(),
		"Chunk_Coordinates_Duplication");

	disconnectedClient.disconnect();
	workerPoolBlocker.release();

	const auto completionDeadline =
		std::chrono::steady_clock::now() + 250ms;
	while (
		std::chrono::steady_clock::now() <
		completionDeadline)
	{
		router.dispatch();
		std::this_thread::sleep_for(5ms);
	}

	spk::Client healthyClient;
	healthyClient.connect("127.0.0.1", router.port());

	const Chunk::Coordinate healthyCoordinate{2, 0, 1};
	Chunk::Protocol::Request::Builder healthyBuilder;
	healthyBuilder.add(healthyCoordinate);
	const auto healthyRequest =
		std::move(healthyBuilder).build();
	healthyClient.send(healthyRequest);

	std::vector<spk::Message> healthyMessages;
	ASSERT_TRUE(
		collectMessages(
			router,
			healthyClient,
			healthyMessages,
			1u));
	ASSERT_EQ(healthyMessages.size(), 1u);

	const Chunk::Protocol::Response healthyResponse(
		healthyMessages.front());
	EXPECT_EQ(
		healthyResponse.requestID(),
		healthyRequest.requestID());
	ASSERT_EQ(healthyResponse.successCount(), 1u);
	EXPECT_EQ(healthyResponse.failureCount(), 0u);
	EXPECT_EQ(
		healthyResponse.success(0u).coordinate,
		healthyCoordinate);

	std::vector<spk::Message> additionalMessages;
	const bool receivedAdditional =
		waitUntil(
			[&] {
				router.dispatch();
				(void)healthyClient.messages().drain(
					additionalMessages);
				return !additionalMessages.empty();
			},
			100ms);
	EXPECT_FALSE(receivedAdditional);

	healthyClient.disconnect();
	router.stop();
	application.stop();
	EXPECT_FALSE(application.failed());
}

TEST(ClientServerIntegration, DuplicateRequestReceivesDiagnosticThenCanonicalResponse)
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

	const Chunk::Coordinate coordinate{-3, 0, 1};
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

	const Chunk::Protocol::Error error(messages[0]);
	EXPECT_EQ(error.requestID(), 700u);
	EXPECT_EQ(
		error.severity(),
		Networking::Diagnostic::Severity::Warning);
	EXPECT_EQ(
		error.message(),
		"Chunk_Coordinates_Duplication");
	ASSERT_EQ(error.coordinateCount(), 1u);
	EXPECT_EQ(error.coordinate(0u), coordinate);

	const Chunk::Protocol::Response response(messages[1]);
	EXPECT_EQ(response.requestID(), 700u);
	ASSERT_EQ(response.successCount(), 1u);
	EXPECT_EQ(response.failureCount(), 0u);
	EXPECT_EQ(response.success(0u).coordinate, coordinate);

	client.disconnect();
	router.stop();
	application.stop();
	EXPECT_FALSE(application.failed());
}

TEST(ClientServerIntegration, MalformedRequestReceivesDiagnosticWithoutChunkResponse)
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

	const Networking::Diagnostic diagnostic(messages.front());
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
				(void)client.messages().drain(additional);
				return !additional.empty();
			},
			100ms);
	EXPECT_FALSE(receivedAdditional);

	client.disconnect();
	router.stop();
	application.stop();
	EXPECT_FALSE(application.failed());
}
