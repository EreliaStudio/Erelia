#include "erelia/client/translator_service.hpp"
#include <filesystem>
#include <Windows.h>
#include <stdexcept>
#include <string>

namespace erelia::client
{
    spk::Translator &translatorService()
    {
        static spk::Translator translator;
        static const bool loaded = [] {
            std::wstring executable(32768, L'\0');
            DWORD length = GetModuleFileNameW(nullptr, executable.data(), static_cast<DWORD>(executable.size()));
            if (!length || length >= executable.size()) throw std::runtime_error("Cannot locate translation resource");
            executable.resize(length);
            translator.append(std::filesystem::path(executable).parent_path() / "translations/en.i18l");
            return true;
        }();
        (void)loaded;
        return translator;
    }
}
