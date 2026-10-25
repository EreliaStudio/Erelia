#pragma once

#include <container/json/object.hpp>

#include <cstdint>
#include <filesystem>
#include <string>
#include <vector>

namespace erelia::core
{
    struct ServiceAddress
    {
        std::string host;
        std::uint16_t port;
    };

    struct WindowConfiguration
    {
        std::string title;
        unsigned int width;
        unsigned int height;
    };

    struct ClientConfiguration
    {
        std::string id;
        ServiceAddress router;
        WindowConfiguration window;
    };

    struct RemoteNodeAddress
    {
        std::string id;
        ServiceAddress endpoint;
    };

    struct ServerRouterConfiguration
    {
        std::string id;
        std::uint16_t listenPort;
        std::vector<RemoteNodeAddress> remoteNodes;
    };

    struct RemoteNodeConfiguration
    {
        std::string id;
        std::uint16_t listenPort;
        spk::JSON::Value settings = spk::JSON::Value::object();
    };

    struct ConfigurationArguments
    {
        std::filesystem::path file;
        bool validateOnly;
    };

    [[nodiscard]] ConfigurationArguments parseConfigurationArguments(int argc, char *const argv[]);
    [[nodiscard]] ClientConfiguration loadClientConfiguration(const std::filesystem::path &file);
    [[nodiscard]] ServerRouterConfiguration loadServerRouterConfiguration(const std::filesystem::path &file);
    [[nodiscard]] RemoteNodeConfiguration loadRemoteNodeConfiguration(const std::filesystem::path &file);
}
