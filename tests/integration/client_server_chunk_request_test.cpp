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
