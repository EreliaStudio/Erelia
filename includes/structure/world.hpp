#pragma once

#include <sparkle.hpp>

class World
{
private:
	static inline std::wstring fileName = L"world.json";
	std::wstring _seed;

public:
	World() :
		_seed(L"EmptySeed")
	{

	}

	void initialize(const std::wstring& p_seed)
	{
		_seed = p_seed;
	}

	void save(const std::filesystem::path& p_rootPath)
	{
		spk::JSON::File outputFile = spk::JSON::File(p_rootPath / fileName);

		outputFile.addAttribute(L"Seed") = _seed;
		
		outputFile.save(p_rootPath / fileName);
	}

	void load(const std::filesystem::path& p_rootPath)
	{
		spk::JSON::File inputFile;

		_seed = inputFile[L"Seed"].as<std::wstring>();
	}
};