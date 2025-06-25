#include "structure/game_info.hpp"

void GameInfo::save(const std::filesystem::path& p_rootPath)
{
	spk::JSON::File outputFile = spk::JSON::File();

	outputFile.root().addAttribute(L"Name") = name;
	outputFile.root().addAttribute(L"IconSprite") = iconSprite;
	
	outputFile.save(p_rootPath / fileName);
}

void GameInfo::load(const std::filesystem::path& p_rootPath)
{
	spk::JSON::File inputFile = spk::JSON::File(p_rootPath / fileName);

	name = inputFile[L"Name"].as<std::wstring>();
	iconSprite = inputFile[L"IconSprite"].as<spk::Vector2UInt>();
}