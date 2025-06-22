#include "structure/world.hpp"

void GameFile::configure(const spk::JSON::File& p_configurationFile)
{
	saveFolder = p_configurationFile[L"SaveFolder"].as<std::wstring>();
}

void GameFile::createNewGameFile(const std::wstring& p_name, const std::wstring& p_seed, const spk::Vector2UInt& p_iconIndex)
{
	if (exist(p_name) == false)
	{
		std::error_code ec;
		std::filesystem::create_directories(gameFolder, ec);
	}
	
	spk::JSON::File gameFile;

	gameFile.root().addAttribute(L"Name") = p_name;
	gameFile.root().addAttribute(L"IconIndex") = p_iconIndex;
	gameFile.save(saveFolder / name / L"configure.json");
	
	World newWorld;
	newWorld.setSeed(p_seed);
	newWorld.save(saveFolder / name);
}

bool exist(const std::wstring& p_name)
{
	std::error_code ec;

	if (!std::filesystem::exists(saveFolder, ec) ||
		(std::filesystem::status(saveFolder, ec).permissions() &
		std::filesystem::perms::owner_write) == std::filesystem::perms::none)
	{
		return false;
	}

	if (p_name.empty() || p_name.find_first_not_of(L" \t\n\r") == std::wstring::npos)
	{
		return false;
	}

	static constexpr std::wstring_view forbidden = LR"(<>:"/\|?*)";
	if (p_name.find_first_of(forbidden) != std::wstring::npos)
	{
		return false;
	}

	const std::filesystem::path gameFolder = saveFolder / p_name;
	if (std::filesystem::exists(gameFolder, ec))
	{
		return false;
	}			

	return true;
}

void GameFile::save()
{
	spk::JSON::File gameFile;

	const std::filesystem::path gameFolder = saveFolder / name;

	std::error_code ec;
	std::filesystem::create_directories(gameFolder, ec);

	gameFile.root().addAttribute(L"Name") = name;
	gameFile.root().addAttribute(L"Seed") = seed;
	gameFile.root().addAttribute(L"IconSprite") = iconSprite;

	gameFile.save(gameFolder / L"save.json");
}

void GameFile::load(std::wstring p_gameName)
{
	spk::JSON::File gameFile;
	gameFile.load(saveFolder / p_gameName / L"save.json");

	name = gameFile[L"Name"].as<std::wstring>();
	seed = gameFile[L"Seed"].as<std::wstring>();
	iconSprite = spk::Vector2UInt(gameFile[L"IconSprite"]);
}