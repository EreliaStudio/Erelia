#include "erelia/core/translation_engine.hpp"

#include <container/json/reader.hpp>
#include <exception.hpp>

#include <exception>
#include <format>
#include <mutex>
#include <string>
#include <unordered_map>
#include <utility>

void TranslationEngine::load(const std::filesystem::path &path)
{
	const spk::JSON::Value document = spk::JSON::Loader::parseFile(path);
	if (document.isObject() == false)
	{
		throw spk::Exception(
			"Translation catalog root must be a JSON object: " +
			path.generic_string());
	}

	std::unordered_map<std::string, std::string> translations;
	translations.reserve(document.size());

	for (const auto &[identifier, value] : document.asObject())
	{
		if (identifier.empty() == true)
		{
			throw spk::Exception(
				"Translation identifier cannot be empty: " +
				path.generic_string());
		}
		if (value.isString() == false)
		{
			throw spk::Exception(
				"Translation value must be a string for identifier '" +
				identifier + "': " + path.generic_string());
		}

		translations.emplace(
			identifier,
			value.as<std::string>());
	}

	const std::unique_lock lock(_mutex);
	_translations = std::move(translations);
}

std::string TranslationEngine::_translate(
	std::string_view identifier,
	std::format_args arguments) const
{
	std::string format;
	{
		const std::shared_lock lock(_mutex);
		const auto iterator = _translations.find(std::string(identifier));
		if (iterator == _translations.end())
		{
			throw spk::Exception(
				"Unknown translation identifier: " +
				std::string(identifier));
		}
		format = iterator->second;
	}

	try
	{
		return std::vformat(format, arguments);
	}
	catch (const std::format_error &)
	{
		throw spk::Exception(
			"Invalid translation format for identifier: " +
				std::string(identifier),
			std::current_exception());
	}
}
