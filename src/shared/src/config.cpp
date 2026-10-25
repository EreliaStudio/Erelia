#include "erelia/shared/config.hpp"
#include <container/json/reader.hpp>
#include <container/json/object.hpp>
#include <stdexcept>
#include <string>
#include <string_view>
#ifdef _WIN32
#include <Windows.h>
#else
#include <unistd.h>
#endif

namespace
{
    std::filesystem::path executableDirectory(char **argv)
    {
#ifdef _WIN32
        std::wstring path(32768, L'\0');
        DWORD length = GetModuleFileNameW(nullptr, path.data(), static_cast<DWORD>(path.size()));
        if (length == 0 || length >= path.size()) throw std::runtime_error("Cannot locate executable");
        path.resize(length);
        return std::filesystem::path(path).parent_path();
#else
        (void)argv;
        return std::filesystem::canonical("/proc/self/exe").parent_path();
#endif
    }
    std::uint16_t port(const spk::JSON::Reader &reader, const char *name)
    {
        auto value = reader.require<std::uint16_t>(name);
        if (!value) throw std::invalid_argument("Port cannot be zero");
        return value;
    }
    erelia::config::Endpoint endpoint(const spk::JSON::Reader &reader)
    {
        reader.forbidUnknown({"host", "port"});
        return {reader.require<std::string>("host"), port(reader, "port")};
    }
    std::filesystem::path logPath(const spk::JSON::Reader &reader)
    {
        return reader.optional<std::string>("logFile", "");
    }
    spk::JSON::Value document(const std::filesystem::path &path)
    {
        return spk::JSON::Value::loadFromFile(path);
    }
}

namespace erelia::config
{
    Launch arguments(int argc, char **argv, const std::string &defaultName)
    {
        if (argc == 1) return {executableDirectory(argv) / defaultName, false};
        if (argc == 3 && std::string_view(argv[1]) == "--config")
            return {argv[2], false};
        if (argc == 3 && std::string_view(argv[1]) == "--validate-config")
            return {argv[2], true};
        throw std::invalid_argument("Usage: [--config <path> | --validate-config <path>]");
    }
    Client client(const std::filesystem::path &path)
    {
        const auto data = document(path);
        const spk::JSON::Reader r(data, path);
        r.forbidUnknown({"router", "timeoutMs", "commandPort", "logFile"});
        Client result{endpoint(r.child("router")), r.optional<std::uint32_t>("timeoutMs", 3000), r.optional<std::uint16_t>("commandPort", 0), logPath(r)};
        if (result.timeoutMs == 0) throw std::invalid_argument("timeoutMs cannot be zero");
        return result;
    }
    Router router(const std::filesystem::path &path)
    {
        const auto data = document(path);
        const spk::JSON::Reader r(data, path);
        r.forbidUnknown({"port", "serverA", "serverB", "logFile"});
        return {port(r, "port"), endpoint(r.child("serverA")), endpoint(r.child("serverB")), logPath(r)};
    }
    Server server(const std::filesystem::path &path)
    {
        const auto data = document(path);
        const spk::JSON::Reader r(data, path);
        r.forbidUnknown({"port", "respond", "logFile"});
        return {port(r, "port"), r.optional<bool>("respond", true), logPath(r)};
    }
}
