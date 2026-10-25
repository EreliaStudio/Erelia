#pragma once
#include <cstdint>
#include <filesystem>
#include <string>

namespace erelia::config
{
    struct Endpoint { std::string host; std::uint16_t port = 0; };
    struct Client { Endpoint router; std::uint32_t timeoutMs = 3000; std::uint16_t commandPort = 0; std::filesystem::path logFile; };
    struct Router { std::uint16_t port = 0; Endpoint serverA; Endpoint serverB; std::filesystem::path logFile; };
    struct Server { std::uint16_t port = 0; bool respond = true; std::filesystem::path logFile; };
    struct Launch { std::filesystem::path file; bool validateOnly = false; };

    [[nodiscard]] Launch arguments(int argc, char **argv, const std::string &defaultName);
    [[nodiscard]] Client client(const std::filesystem::path &path);
    [[nodiscard]] Router router(const std::filesystem::path &path);
    [[nodiscard]] Server server(const std::filesystem::path &path);
}
