#include <gtest/gtest.h>

#include <filesystem>
#include "erelia/client/application.hpp"

// Bootstrap test: attach M0 Test ID and requirement when approved.
TEST(ClientConfigurationLocation, IsRelativeToTheExecutableNotTheWorkingDirectory)
{
    const std::filesystem::path installedExecutable = std::filesystem::path("C:/Erelia/bin") / "EreliaClient.exe";
    EXPECT_EQ(
        erelia::client::configurationFilePath(installedExecutable),
        std::filesystem::path("C:/Erelia/bin/client.json"));
}
