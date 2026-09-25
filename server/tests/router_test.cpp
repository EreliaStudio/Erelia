#include "erelia/server/router.hpp"

#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <gtest/gtest.h>
#include <network/remote_node.hpp>
#include <type/uuid.hpp>

#include <chrono>
#include <filesystem>
#include <fstream>
#include <sstream>
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
				("erelia-router-" +
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

	[[nodiscard]] std::string readText(
		const std::filesystem::path &path)
	{
		std::ifstream stream(path, std::ios::binary);
		std::ostringstream content;
		content << stream.rdbuf();
		return content.str();
	}

	[[nodiscard]] bool waitUntilConnected(
		Router &router,
		std::chrono::milliseconds timeout = 2s)
	{
		const auto deadline =
			std::chrono::steady_clock::now() + timeout;
		while (std::chrono::steady_clock::now() < deadline)
		{
			router.dispatch();
			if (router.isNodeConnected("terrain"))
			{
				return true;
			}
			std::this_thread::sleep_for(5ms);
		}
		return false;
	}
}

TEST(ServerRouterConfiguration, LoadsExactExternalContract)
{
	const TemporaryJsonFile file(
		R"({
			"server config":{"port":0,"nodeReconnectDelayMs":25},
			"nodes":[{"name":"terrain","address":"127.0.0.1","port":2551}]
		})");

	const Router::Configuration configuration =
		Router::Configuration::load(
			file.path().string());

	EXPECT_EQ(configuration.port, 0u);
	EXPECT_EQ(configuration.nodeReconnectDelay, 25ms);
	ASSERT_EQ(configuration.nodes.size(), 1u);
	EXPECT_EQ(configuration.nodes[0].name, "terrain");
	EXPECT_EQ(configuration.nodes[0].address, "127.0.0.1");
	EXPECT_EQ(configuration.nodes[0].port, 2551u);
}

TEST(ServerRouterConfiguration, RejectsInvalidContracts)
{
	const std::string fixtures[] = {
		R"({"server config":{"port":0},"nodes":[]})",
		R"({"server config":{"port":0,"nodeReconnectDelayMs":0},"nodes":[]})",
		R"({"server config":{"port":70000,"nodeReconnectDelayMs":1},"nodes":[]})",
		R"({"server config":{"port":0,"nodeReconnectDelayMs":1,"extra":1},"nodes":[]})",
		R"({"server config":{"port":0,"nodeReconnectDelayMs":1},"nodes":[{"name":"","address":"127.0.0.1","port":1}]})",
		R"({"server config":{"port":0,"nodeReconnectDelayMs":1},"nodes":[{"name":"terrain","address":"","port":1}]})",
		R"({"server config":{"port":0,"nodeReconnectDelayMs":1},"nodes":[{"name":"terrain","address":"127.0.0.1","port":0}]})",
		R"({"server config":{"port":0,"nodeReconnectDelayMs":1},"nodes":[{"name":"terrain","address":"127.0.0.1","port":1},{"name":"terrain","address":"127.0.0.1","port":2}]})",
		R"({"server config":{"port":0,"nodeReconnectDelayMs":1},"nodes":[],"extra":true})"};

	for (const std::string &fixture : fixtures)
	{
		const TemporaryJsonFile file(fixture);
		EXPECT_THROW(
			(void)Router::Configuration::load(
				file.path().string()),
			spk::Exception);
	}
}

TEST(ServerRouterRuntime, ConnectsRemoteNodeAndRestarts)
{
	spk::RemoteNode::Endpoint endpoint;
	endpoint.start(0);

	Router router(
		Router::Configuration{
			.port = 0,
			.nodeReconnectDelay = 10ms,
			.nodes = {
				{"terrain", "127.0.0.1", endpoint.port()}}});

	router.start();
	EXPECT_TRUE(router.isRunning());
	EXPECT_NE(router.port(), 0u);
	ASSERT_TRUE(waitUntilConnected(router));

	router.stop();
	EXPECT_FALSE(router.isRunning());
	EXPECT_FALSE(router.isNodeConnected("terrain"));

	router.start();
	ASSERT_TRUE(waitUntilConnected(router));
	router.stop();
	endpoint.stop();
}

TEST(ServerRouterRuntime, MissingNodeWarnsAndReconnectsLater)
{
	spk::RemoteNode::Endpoint portProbe;
	portProbe.start(0);
	const std::uint16_t terrainPort = portProbe.port();
	portProbe.stop();

	Router router(
		Router::Configuration{
			.port = 0,
			.nodeReconnectDelay = 20ms,
			.nodes = {
				{"terrain", "127.0.0.1", terrainPort}}});

	const std::filesystem::path logPath =
		std::filesystem::temp_directory_path() /
		("erelia-router-warning-" +
		 spk::UUID::generate().toString() +
		 ".log");

	spk::Logger::instance().muteConsole();
	{
		auto output =
			spk::Logger::instance().addOutput(
				logPath,
				spk::Logger::Level::Warning);
		router.start();
	}
	spk::Logger::instance().unmuteConsole();

	EXPECT_TRUE(router.isRunning());
	EXPECT_FALSE(router.isNodeConnected("terrain"));

	const std::string log = readText(logPath);
	EXPECT_NE(log.find("[Warning]"), std::string::npos);
	EXPECT_NE(log.find("terrain"), std::string::npos);
	EXPECT_NE(
		log.find(std::to_string(terrainPort)),
		std::string::npos);

	spk::RemoteNode::Endpoint endpoint;
	endpoint.start(terrainPort);
	EXPECT_TRUE(waitUntilConnected(router));

	router.stop();
	endpoint.stop();

	std::error_code error;
	std::filesystem::remove(logPath, error);
}

TEST(ServerRouterRuntime, ReconnectsAfterEstablishedNodeDisconnects)
{
	spk::RemoteNode::Endpoint endpoint;
	endpoint.start(0);
	const std::uint16_t terrainPort = endpoint.port();

	Router router(
		Router::Configuration{
			.port = 0,
			.nodeReconnectDelay = 20ms,
			.nodes = {
				{"terrain", "127.0.0.1", terrainPort}}});

	router.start();
	ASSERT_TRUE(waitUntilConnected(router));

	endpoint.stop();

	const auto disconnectDeadline =
		std::chrono::steady_clock::now() + 2s;
	while (router.isNodeConnected("terrain") &&
		   std::chrono::steady_clock::now() < disconnectDeadline)
	{
		router.dispatch();
		std::this_thread::sleep_for(5ms);
	}
	ASSERT_FALSE(router.isNodeConnected("terrain"));

	endpoint.start(terrainPort);
	EXPECT_TRUE(waitUntilConnected(router));

	router.stop();
	endpoint.stop();
}
