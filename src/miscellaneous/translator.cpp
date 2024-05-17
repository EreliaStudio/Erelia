#include "translator.hpp"

#include "sparkle.hpp"

void Translator::load(const std::filesystem::path &filePath)
{
	spk::JSON::File jsonFile(filePath);
	if (!jsonFile.contains("Translations"))
	{
		throw std::runtime_error("Translations key is missing in the JSON file.");
	}

	const spk::JSON::Object &transArray = jsonFile["Translations"];
	if (!transArray.isArray())
	{
		throw std::runtime_error("Expected an array for translations.");
	}

	translations.clear();

	for (const auto &transObject : transArray.asArray())
	{
		if (!transObject->isObject() || transObject->size() != 1)
		{
			throw std::runtime_error("Each translation entry must be an object with exactly one key-value pair.");
		}
		
		const auto &members = transObject->members();
		const std::string key = members.begin()->first;
		const spk::JSON::Object *valueObj = members.begin()->second;

		if (!valueObj->isUnit() || !valueObj->hold<std::string>())
		{
			throw std::runtime_error("Translation values must be string types.");
		}
		translations[key] = valueObj->as<std::string>();
	}
}

std::string Translator::translate(const std::string &key)
{
	auto it = translations.find(key);
	if (it == translations.end())
	{
		return key;
	}
	return it->second;
}