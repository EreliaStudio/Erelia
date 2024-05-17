#pragma once

#include <filesystem>
#include <unordered_map>
#include <string>
#include <exception>

class Translator
{
private:
    static inline std::unordered_map<std::string, std::string> translations;

public:
    Translator() = delete;

    static void load(const std::filesystem::path &filePath);
    static std::string translate(const std::string &key);
};

