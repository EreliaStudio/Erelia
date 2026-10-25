#include "erelia/core/configuration.hpp"

#include <container/json/object.hpp>
#include <container/json/reader.hpp>

#include <set>
#include <stdexcept>
#include <string_view>
#include <utility>

namespace
{
    using Reader = spk::JSON::Reader;

    std::string requiredNonemptyString(const Reader &reader, const char *key)
    {
        std::string value = reader.require<std::string>(key);
        if (value.empty())
        {
            throw std::invalid_argument(reader.pathFor(key) + " must not be empty");
        }
        return value;
    }

    std::uint16_t requiredPort(const Reader &reader, const char *key)
    {
        const auto port = reader.require<std::uint16_t>(key);
        if (port == 0)
        {
            throw std::invalid_argument(reader.pathFor(key) + " must be in the range 1..65535");
        }
        return port;
    }

    void requireKind(const Reader &reader, std::string_view kind)
    {
        if (reader.require<std::string>("kind") != kind)
        {
            throw std::invalid_argument(reader.file().string() + ": incompatible executable configuration kind");
        }
    }

    erelia::core::ServiceAddress readAddress(const Reader &reader)
    {
        reader.forbidUnknown({"host", "port"});
        return {
            .host = requiredNonemptyString(reader, "host"),
            .port = requiredPort(reader, "port")
        };
    }

    spk::JSON::Value loadDocument(const std::filesystem::path &file)
    {
        return spk::JSON::Value::loadFromFile(file);
    }
}

namespace erelia::core
{
    ConfigurationArguments parseConfigurationArguments(int argc, char *const argv[])
    {
        if (argc != 3 || argv == nullptr || argv[1] == nullptr || argv[2] == nullptr)
        {
            throw std::invalid_argument("Expected --config <path.json> or --validate-config <path.json>");
        }
        const std::string_view flag(argv[1]);
        if (flag != "--config" && flag != "--validate-config")
        {
            throw std::invalid_argument("Expected --config <path.json> or --validate-config <path.json>");
        }
        if (std::string_view(argv[2]).empty())
        {
            throw std::invalid_argument("Configuration file path must not be empty");
        }
        return {.file = argv[2], .validateOnly = (flag == "--validate-config")};
    }

    ClientConfiguration loadClientConfiguration(const std::filesystem::path &file)
    {
        const auto document = loadDocument(file);
        const Reader root(document, file);
        root.forbidUnknown({"kind", "id", "router", "window"});
        requireKind(root, "client");

        const Reader window = root.child("window");
        window.forbidUnknown({"title", "width", "height"});
        WindowConfiguration windowConfiguration{
            .title = requiredNonemptyString(window, "title"),
            .width = window.require<unsigned int>("width"),
            .height = window.require<unsigned int>("height")
        };
        if (windowConfiguration.width == 0 || windowConfiguration.height == 0)
        {
            throw std::invalid_argument(file.string() + ": window dimensions must be positive");
        }

        return {
            .id = requiredNonemptyString(root, "id"),
            .router = readAddress(root.child("router")),
            .window = std::move(windowConfiguration)
        };
    }

    ServerRouterConfiguration loadServerRouterConfiguration(const std::filesystem::path &file)
    {
        const auto document = loadDocument(file);
        const Reader root(document, file);
        root.forbidUnknown({"kind", "id", "listenPort", "remoteNodes"});
        requireKind(root, "server-router");

        ServerRouterConfiguration result{
            .id = requiredNonemptyString(root, "id"),
            .listenPort = requiredPort(root, "listenPort")
        };
        std::set<std::string> names;
        for (const Reader &node : root.childArray("remoteNodes"))
        {
            node.forbidUnknown({"id", "host", "port"});
            RemoteNodeAddress entry{
                .id = requiredNonemptyString(node, "id"),
                .endpoint = {
                    .host = requiredNonemptyString(node, "host"),
                    .port = requiredPort(node, "port")
                }
            };
            if (!names.insert(entry.id).second)
            {
                throw std::invalid_argument(file.string() + ": duplicate remote node ID " + entry.id);
            }
            result.remoteNodes.push_back(std::move(entry));
        }
        return result;
    }

    RemoteNodeConfiguration loadRemoteNodeConfiguration(const std::filesystem::path &file)
    {
        const auto document = loadDocument(file);
        const Reader root(document, file);
        root.forbidUnknown({"kind", "id", "listenPort", "settings"});
        requireKind(root, "remote-node");

        spk::JSON::Value settings = spk::JSON::Value::object();
        if (root.contains("settings"))
        {
            settings = root.value().at("settings");
            if (!settings.isObject())
            {
                throw std::invalid_argument(file.string() + ": $.settings must be a JSON object");
            }
        }
        return {
            .id = requiredNonemptyString(root, "id"),
            .listenPort = requiredPort(root, "listenPort"),
            .settings = std::move(settings)
        };
    }
}
