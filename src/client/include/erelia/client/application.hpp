#pragma once

#include <filesystem>

namespace erelia::client
{
    [[nodiscard]] std::filesystem::path configurationFilePath(const std::filesystem::path &executablePath);
    int runApplication();
}
