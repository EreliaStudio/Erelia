#pragma once

#include <sparkle.hpp>

#include "structure/game_info.hpp"
#include "structure/tilemap.hpp"
#include "structure/actor_map.hpp"
#include "structure/player.hpp"

struct Context : public spk::Singleton<Context>
{
	static inline std::filesystem::path saveFolder = "resources/saves";

	static void configure(const spk::JSON::File& p_configurationFile);

	static bool exist(const std::wstring& p_name);
	static std::filesystem::path composeSaveFolderPath(const std::wstring& p_name);

	GameInfo gameInfo;
	Tilemap tilemap;
	ActorMap actorMap;
	spk::SafePointer<Player> player;
	
	Context(const std::wstring& p_name, const std::wstring& p_seed, const spk::Vector2UInt& p_iconIndex);
	Context(const std::wstring& p_name = L"");

	void reset(const std::wstring& p_name, const std::wstring& p_seed, const spk::Vector2UInt& p_iconIndex);
	
	void clear();
	void save();
	bool isLoadable();
	void load(std::wstring p_gameName);
};