#pragma once

#include <sparkle.hpp>

struct GameInfo
{
	static inline std::wstring fileName = L"info.json";

	std::wstring name = L"";
	spk::Vector2UInt iconSprite = {0, 0};

	void save(const std::filesystem::path& p_rootPath);
	void load(const std::filesystem::path& p_rootPath);
};