#include "generating_world_provider.hpp"
#include "terrain_node.hpp"
#include "terrain_node_application.hpp"
#include "terrain_service.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>
#include <type/uuid.hpp>

#include <chrono>
#include <csignal>
#include <exception>
#include <filesystem>
#include <fstream>
#include <string>
#include <thread>

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

TEST(TerrainNodeRuntime, ServiceEndpointHasExactlyOneDispatcher)
{
	{
		TerrainNode node({.port = 0});
		EXPECT_THROW((void)TerrainNode({.port = 0}), spk::Exception);
		node.start();
		EXPECT_EQ(node.port(), Service::terrainEndpoint().port());
	}
	EXPECT_EQ(Service::terrainEndpoint().isRunning(), false);
	EXPECT_NO_THROW((void)TerrainNode({.port = 0}));
}

TEST(TerrainWorldCollection, GeneratesDefinedWorlds)
{
	WorldCollection &worlds =
		Service::terrainWorldCollection();
	auto &provider =
		static_cast<GeneratingWorldProvider &>(
			worlds.provider());
	const WorldIdentifier identifier{
		.name = "test.terrain.world"};

	(void)worlds.remove(identifier);
	provider.define(
		{
			.identifier = identifier,
			.generatorType = GeneratingWorldProvider::Type::Prototype,
			.family = "test"});

	World *world =
		worlds.world(identifier);

	ASSERT_NE(world, nullptr);
	EXPECT_EQ(world->identifier(), identifier);
	EXPECT_EQ(
		worlds.world(identifier),
		world);

	EXPECT_EQ(
		worlds.remove(identifier),
		true);
}

TEST(GeneratingWorldProvider, RejectsUndefinedWorld)
{
	WorldCollection worlds(
		GeneratingWorldProvider{});

	EXPECT_THROW(
		(void)worlds.world(
			WorldIdentifier{
				.name = "undefined.world"}),
		spk::Exception);
	EXPECT_EQ(worlds.size(), 0u);
}
