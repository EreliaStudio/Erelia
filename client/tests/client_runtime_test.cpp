#include "erelia/client/client_configuration.hpp"
#include "erelia/client/client_runtime.hpp"
#include "erelia/client/client_world.hpp"
#include "erelia/client/command/connect_command.hpp"
#include "erelia/client/main_interface.hpp"
#include "erelia/client/service.hpp"
#include "erelia/client/widget_order.hpp"
#include "erelia/client/world_manager.hpp"

#include <core/application.hpp>
#include <diagnostics/logger.hpp>
#include <exception.hpp>
#include <gtest/gtest.h>
#include <type/uuid.hpp>

#include <chrono>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <string>

namespace
{
	class TemporaryJsonFile final
	{
	private:
		std::filesystem::path _path;

	public:
		explicit TemporaryJsonFile(const std::string &content)
		{
			_path = std::filesystem::temp_directory_path() /
					("erelia-client-" + spk::UUID::generate().toString() + ".json");
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
}

TEST(ClientConfiguration, LoadsExactExternalContract)
{
	const TemporaryJsonFile file(
		R"({"server config":{"address":"127.0.0.1","port":2550,"retryDelayMs":15000},"terrain config":{"viewRange":1,"unloadRange":2}})");

	const ClientConfiguration configuration =
		ClientConfiguration::load(file.path());

	EXPECT_EQ(configuration.server.address, "127.0.0.1");
	EXPECT_EQ(configuration.server.port, 2550u);
	EXPECT_EQ(configuration.retryDelay, std::chrono::milliseconds(15000));
}

TEST(ClientConfiguration, RejectsInvalidContracts)
{
	const std::string fixtures[] = {
		R"({,"terrain config":{"viewRange":1,"unloadRange":2}})",
		R"({"server config":{"address":"127.0.0.1","port":2550},"terrain config":{"viewRange":1,"unloadRange":2}})",
		R"({"server config":{"port":2550,"retryDelayMs":15000},"terrain config":{"viewRange":1,"unloadRange":2}})",
		R"({"server config":{"address":"127.0.0.1","retryDelayMs":15000},"terrain config":{"viewRange":1,"unloadRange":2}})",
		R"({"server config":{"address":"","port":2550,"retryDelayMs":15000},"terrain config":{"viewRange":1,"unloadRange":2}})",
		R"({"server config":{"address":"127.0.0.1","port":0,"retryDelayMs":15000},"terrain config":{"viewRange":1,"unloadRange":2}})",
		R"({"server config":{"address":"127.0.0.1","port":70000,"retryDelayMs":15000},"terrain config":{"viewRange":1,"unloadRange":2}})",
		R"({"server config":{"address":"127.0.0.1","port":2550,"retryDelayMs":0},"terrain config":{"viewRange":1,"unloadRange":2}})",
		R"({"server config":{"address":"127.0.0.1","port":2550,"retryDelayMs":-1},"terrain config":{"viewRange":1,"unloadRange":2}})",
		R"({"server config":{"address":"127.0.0.1","port":2550,"retryDelayMs":15000,"extra":true},"terrain config":{"viewRange":1,"unloadRange":2}})",
		R"({"server config":{"address":"127.0.0.1","port":2550,"retryDelayMs":15000},"extra":true,"terrain config":{"viewRange":1,"unloadRange":2}})"};

	for (const std::string &fixture : fixtures)
	{
		const TemporaryJsonFile file(fixture);
		EXPECT_THROW(
			(void)ClientConfiguration::load(file.path()),
			spk::Exception);
	}
}

TEST(ClientConfiguration, ValidationExceptionRemainsStable)
{
	const TemporaryJsonFile file(
		R"({"server config":{"address":"","port":2550,"retryDelayMs":15000},"terrain config":{"viewRange":1,"unloadRange":2}})");

	try
	{
		(void)ClientConfiguration::load(file.path());
		FAIL() << "Expected spk::Exception";
	} catch (const spk::Exception &exception)
	{
		EXPECT_EQ(
			exception.message(),
			"Client Server address cannot be empty");
	}
}

TEST(ClientConsole, OrdinarySubmissionUsesUserValueA)
{
	spk::Logger::Level receivedLevel = spk::Logger::Level::Trace;
	std::string receivedMessage;
	auto contract = spk::logger.subscribeToEntry(
		[&](const spk::Logger::Level &level, const std::string &message) {
			receivedLevel = level;
			receivedMessage = message;
		});

	SPK_LOG(UserValueA) << "player input" << std::endl;

	EXPECT_EQ(receivedLevel, spk::Logger::Level::UserValueA);
	EXPECT_EQ(receivedMessage, "player input");
}

TEST(ClientRuntime, ConnectionStartsPlayerLoadingLifecycle)
{
	spk::Application application;
	spk::Window &mainWindow = application.createWindow(
		"main",
		spk::Window::Configuration{
			.title = "Erelia",
			.area = spk::Rect2D{
				.anchor = {0, 0},
				.size = {640, 480}}});

	int loadingRequests = 0;
	int readyPlayers = 0;
	auto loading = Service::clientEventCenter().playerLoadingRequested().subscribe([&] {
		++loadingRequests;
	});
	auto ready = Service::clientEventCenter().playerReady().subscribe(
		[&](const PlayerInformation &) {
			++readyPlayers;
		});

	ClientRuntime runtime(
		{"127.0.0.1", 1},
		std::chrono::milliseconds(1),
		&mainWindow.root());
	ClientWorld clientWorld({.name = "test.lifecycle"}, runtime.networkManager());

	Service::clientEventCenter().worldChanged().trigger(&clientWorld);
	Service::clientEventCenter().clientConnected().trigger();

	EXPECT_EQ(runtime.world(), &clientWorld);
	EXPECT_EQ(loadingRequests, 1);
	EXPECT_EQ(readyPlayers, 1);
}

TEST(ClientBootstrap, InitializesSeparatedClientRootsFromConfiguredWindowGeometry)
{
	spk::Application application;
	spk::Window &mainWindow = application.createWindow(
		"main",
		spk::Window::Configuration{
			.title = "Erelia",
			.area = spk::Rect2D{
				.anchor = {0, 0},
				.size = {640, 480}}});

	ClientRuntime runtime(
		{"127.0.0.1", 1},
		std::chrono::milliseconds(1),
		&mainWindow.root());
	ClientWorld clientWorld({.name = "test.bootstrap"}, runtime.networkManager());
	WorldManager world(
		"/WorldManager",
		{1, 2},
		&mainWindow.root());
	MainInterface interface(
		"/MainInterface",
		&mainWindow.root());
	interface.console().commandParser().addCommand<ConnectCommand>();

	world.setGeometry(mainWindow.root().geometry());
	Service::clientEventCenter().worldChanged().trigger(&clientWorld);
	interface.setGeometry(mainWindow.root().geometry());

	const spk::Rect2D expectedGeometry{
		.anchor = {0, 0},
		.size = {640, 480}};

	EXPECT_EQ(mainWindow.root().geometry(), expectedGeometry);
	EXPECT_EQ(world.geometry(), expectedGeometry);
	EXPECT_EQ(interface.geometry(), expectedGeometry);
	EXPECT_EQ(interface.name(), "/MainInterface");
	EXPECT_EQ(
		runtime.connectionManager().name(),
		"/ClientRuntime/ConnectionManager");
	EXPECT_EQ(
		runtime.connectionManager().zOrder(),
		Client::WidgetOrder::Connection);
	EXPECT_EQ(
		runtime.networkManager().zOrder(),
		Client::WidgetOrder::Network);
	EXPECT_EQ(world.zOrder(), Client::WidgetOrder::World);
	EXPECT_EQ(interface.zOrder(), Client::WidgetOrder::Interface);

	const auto &rootChildren = mainWindow.root().children();
	ASSERT_EQ(rootChildren.size(), 4u);
	EXPECT_EQ(rootChildren[0], &runtime.connectionManager());
	EXPECT_EQ(rootChildren[1], &runtime.networkManager());
	EXPECT_EQ(rootChildren[2], &world);
	EXPECT_EQ(rootChildren[3], &interface);

	EXPECT_EQ(runtime.world(), &clientWorld);
	EXPECT_EQ(world.world(), &clientWorld);
	EXPECT_EQ(world.engine(), &clientWorld.engine());
	EXPECT_EQ(world.player(), nullptr);
	EXPECT_EQ(
		interface.console().name(),
		"/MainInterface/Console");
	EXPECT_EQ(
		interface.console().entryView().name(),
		"/MainInterface/Console/entries");
	EXPECT_EQ(
		interface.console().commandEntry().name(),
		"/MainInterface/Console/command");
	EXPECT_EQ(
		interface.console()
			.commandParser()
			.command<ConnectCommand>()
			.name(),
		"connect");
	EXPECT_EQ(interface.console().geometry(), expectedGeometry);
}

TEST(ClientWorldLifecycle, WorldChangedRebindsRuntimeAndEngineWidget)
{
	spk::Application application;
	spk::Window &mainWindow = application.createWindow(
		"main",
		spk::Window::Configuration{
			.title = "Erelia",
			.area = spk::Rect2D{
				.anchor = {0, 0},
				.size = {640, 480}}});

	ClientRuntime runtime(
		{"127.0.0.1", 1},
		std::chrono::milliseconds(1),
		&mainWindow.root());
	ClientWorld first({.name = "test.first"}, runtime.networkManager());
	ClientWorld second({.name = "test.second"}, runtime.networkManager());
	WorldManager manager(
		"/WorldManager",
		{1, 2},
		&mainWindow.root());

	World::Chunks *currentChunks = nullptr;
	auto collectionBinding =
		Service::clientEventCenter().worldChanged().subscribe(
			[&](World *world) {
				currentChunks =
					world == nullptr ? nullptr : world->chunkCollection();
			});

	Service::clientEventCenter().worldChanged().trigger(&first);
	EXPECT_EQ(runtime.world(), &first);
	EXPECT_EQ(manager.world(), &first);
	EXPECT_EQ(manager.engine(), &first.engine());
	EXPECT_EQ(currentChunks, first.chunkCollection());

	Service::clientEventCenter().worldChanged().trigger(&second);
	EXPECT_EQ(runtime.world(), &second);
	EXPECT_EQ(manager.world(), &second);
	EXPECT_EQ(manager.engine(), &second.engine());
	EXPECT_EQ(currentChunks, second.chunkCollection());

	Service::clientEventCenter().worldChanged().trigger(nullptr);
	EXPECT_EQ(runtime.world(), nullptr);
	EXPECT_EQ(manager.world(), nullptr);
	EXPECT_EQ(manager.engine(), nullptr);
	EXPECT_EQ(currentChunks, nullptr);
}

TEST(ClientWorldLifecycle, DetachesBeforeRemovingActiveNamedWorld)
{
	spk::Application application;
	spk::Window &mainWindow = application.createWindow(
		"main",
		spk::Window::Configuration{
			.title = "Erelia",
			.area = spk::Rect2D{
				.anchor = {0, 0},
				.size = {640, 480}}});

	ClientRuntime runtime(
		{"127.0.0.1", 1},
		std::chrono::milliseconds(1),
		&mainWindow.root());
	WorldManager manager(
		"/WorldManager",
		{1, 2},
		&mainWindow.root());

	WorldCollection &worlds =
		Service::clientWorldCollection();
	const WorldIdentifier identifier{
		.name = "test.client.world"};
	(void)worlds.remove(identifier);
	World *world =
		worlds.world(identifier);

	ASSERT_NE(dynamic_cast<ClientWorld *>(world), nullptr);
	EXPECT_EQ(world->identifier(), identifier);
	Service::clientEventCenter().worldChanged().trigger(world);
	ASSERT_EQ(runtime.world(), world);
	ASSERT_EQ(manager.world(), world);
	ASSERT_EQ(manager.engine(), &world->engine());

	Service::clientEventCenter().worldChanged().trigger(nullptr);
	EXPECT_EQ(runtime.world(), nullptr);
	EXPECT_EQ(manager.world(), nullptr);
	EXPECT_EQ(manager.engine(), nullptr);

	EXPECT_EQ(
		worlds.remove(identifier),
		true);
}
