#include "structure/context.hpp"

void Context::configure(const spk::JSON::File& p_configurationFile)
{
	saveFolder = p_configurationFile[L"SaveFolder"].as<std::wstring>();
}

std::filesystem::path Context::composeSaveFolderPath(const std::wstring& p_name)
{
	return (saveFolder / p_name);
}

bool Context::exist(const std::wstring& p_name)
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
		return true;
	}			

	return false;
}
	
Context::Context(const std::wstring& p_name, const std::wstring& p_seed, const spk::Vector2UInt& p_iconIndex) :
	gameInfo({p_name, p_iconIndex})
{
	tilemap.setSeed(p_seed);
}

void Context::reset(const std::wstring& p_name, const std::wstring& p_seed, const spk::Vector2UInt& p_iconIndex)
{
	gameInfo = {p_name, p_iconIndex};

	tilemap.reset();
	tilemap.setSeed(p_seed);

	actorMap.clear();

	player = actorMap.addActor(0, std::make_unique<Player>()).upCast<Player>();
	player->place({0.5f, 0.5f});
}
	
Context::Context(const std::wstring& p_name)
{
	gameInfo.name = p_name;
}

void Context::clear()
{
	gameInfo = GameInfo();
	tilemap = Tilemap();
}
	
void Context::save()
{
	spk::JSON::File gameFile;

	const std::filesystem::path gameFolder = saveFolder / gameInfo.name;

	std::error_code ec;
	std::filesystem::create_directories(gameFolder, ec);

	gameInfo.save(gameFolder);
	tilemap.save(gameFolder);
}

void Context::load(std::wstring p_gameName)
{
	const std::filesystem::path gameFolder = saveFolder / p_gameName;

	gameInfo.load(gameFolder);
	tilemap.load(gameFolder);
}