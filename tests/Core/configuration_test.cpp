#include <gtest/gtest.h>

#include <filesystem>
#include <stdexcept>
#include "erelia/core/configuration.hpp"

namespace
{
    const std::filesystem::path examples = ERELIA_CONFIG_EXAMPLES_DIR;
    const std::filesystem::path fixtures = ERELIA_CONFIG_FIXTURES_DIR;
}

// Temporary bootstrap tests. Replace these comments with approved M0 Test IDs
// and requirement references when the first M1 infrastructure Story authorizes them.
TEST(ConfigurationTest, EachInstanceLoadsOnlyItsOwnConfiguration)
{
    const auto client = erelia::core::loadClientConfiguration(examples / "client.json");
    EXPECT_EQ(client.id, "local-client");
    EXPECT_EQ(client.router.host, "127.0.0.1");
    EXPECT_EQ(client.router.port, 43000);
    EXPECT_EQ(client.window.width, 960);

    const auto router = erelia::core::loadServerRouterConfiguration(examples / "router.json");
    EXPECT_EQ(router.id, "main-router");
    ASSERT_EQ(router.remoteNodes.size(), 2);
    EXPECT_EQ(router.remoteNodes[0].id, "world");
    EXPECT_EQ(router.remoteNodes[1].endpoint.port, 43020);

    const auto world = erelia::core::loadRemoteNodeConfiguration(examples / "world-node.json");
    const auto chat = erelia::core::loadRemoteNodeConfiguration(examples / "chat-node.json");
    EXPECT_NE(world.id, chat.id);
    EXPECT_NE(world.listenPort, chat.listenPort);
    EXPECT_TRUE(world.settings.isObject());

    const auto example = erelia::core::loadRemoteNodeConfiguration(examples / "example-node.json");
    EXPECT_EQ(example.id, "example");
    EXPECT_TRUE(example.settings.isObject());
}

TEST(ConfigurationTest, ConfigurationKindCannotBeUsedByDifferentExecutable)
{
    EXPECT_THROW(
        erelia::core::loadClientConfiguration(fixtures / "invalid-client-kind.json"),
        std::invalid_argument);
}

TEST(ConfigurationTest, ServerExecutablesAcceptExplicitConfigPath)
{
    char exe[] = "EreliaServerRouter";
    char flag[] = "--config";
    char filename[] = "router.json";
    char *argv[] = {exe, flag, filename};
    const auto args = erelia::core::parseConfigurationArguments(3, argv);
    EXPECT_EQ(args.file, std::filesystem::path("router.json"));
    EXPECT_FALSE(args.validateOnly);

    char validate[] = "--validate-config";
    argv[1] = validate;
    EXPECT_TRUE(erelia::core::parseConfigurationArguments(3, argv).validateOnly);
    EXPECT_THROW(erelia::core::parseConfigurationArguments(1, argv), std::invalid_argument);
}

TEST(ConfigurationTest, NodeSettingsMustBeAJsonObject)
{
    EXPECT_THROW(
        erelia::core::loadRemoteNodeConfiguration(fixtures / "invalid-node-settings.json"),
        std::invalid_argument);
}
