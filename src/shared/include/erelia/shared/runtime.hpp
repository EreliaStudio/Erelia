#pragma once
#include <atomic>
#include <csignal>
#include <filesystem>
#include <memory>
#include <diagnostics/logger.hpp>

namespace erelia::runtime
{
    inline std::atomic_bool running = true;
    inline void stop(int) { running.store(false); }
    inline void signals()
    {
        running = true;
        std::signal(SIGINT, stop);
        std::signal(SIGTERM, stop);
    }
    inline spk::Logger::Output logTo(const std::filesystem::path &path)
    {
        if (path.empty()) return {};
        return spk::logger.addOutput(path, spk::Logger::Level::Trace);
    }
}
