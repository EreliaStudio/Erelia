#include "erelia/client/application.hpp"

#include "erelia/core/configuration.hpp"

#include "core/application.hpp"

#include <filesystem>
#include <stdexcept>
#include <string>

#ifdef _WIN32
#include <Windows.h>
#endif

namespace
{
    [[nodiscard]] std::filesystem::path executablePath()
    {
#ifdef _WIN32
        std::wstring buffer(260, L'\0');
        while (true)
        {
            const DWORD length = GetModuleFileNameW(nullptr, buffer.data(), static_cast<DWORD>(buffer.size()));
            if (length == 0)
            {
                throw std::runtime_error("Unable to locate the Erelia Client executable");
            }
            if (length < buffer.size())
            {
                return std::filesystem::path(std::wstring(buffer.data(), length));
            }
            if (buffer.size() >= 32768)
            {
                throw std::runtime_error("Erelia Client executable path exceeds Windows limits");
            }
            buffer.resize(buffer.size() * 2);
        }
#else
        throw std::runtime_error("Erelia Client is currently supported on Windows only");
#endif
    }
}

namespace erelia::client
{
    std::filesystem::path configurationFilePath(const std::filesystem::path &executablePath)
    {
        return executablePath.parent_path() / "client.json";
    }

    int runApplication()
    {
        const core::ClientConfiguration settings =
            core::loadClientConfiguration(configurationFilePath(executablePath()));

        spk::Application application;
        spk::Window::Configuration configuration{
            .title = settings.window.title,
            .area = {.anchor = {80, 80}, .size = {settings.window.width, settings.window.height}}
        };
        static_cast<void>(application.createWindow("main", configuration));
        return application.run();
    }
}
