#pragma once

#include <sparkle.hpp>

struct GameFile : public spk::Singleton<GameFile>
{
	static inline std::filesystem::path saveFolder = "resources/saves";

	static void configure(const spk::JSON::File& p_configurationFile);

	static bool exist(const std::wstring& p_name);
	static std::filesystem composeSaveFolderPath(const std::wstring& p_path);
	static void createNewGameFile(const std::wstring& p_name, const spk::Vector2UInt& p_iconIndex);

	std::wstring name;
	spk::Vector2UInt iconSprite;

	void save();
	void load(std::wstring p_gameName);
};