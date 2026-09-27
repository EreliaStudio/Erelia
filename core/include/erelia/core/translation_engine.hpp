#pragma once

#include <filesystem>
#include <format>
#include <shared_mutex>
#include <string>
#include <string_view>
#include <unordered_map>
#include <utility>

class TranslationEngine
{
private:
	std::unordered_map<std::string, std::string> _translations;
	mutable std::shared_mutex _mutex;

	[[nodiscard]] std::string _translate(
		std::string_view identifier,
		std::format_args arguments) const;

public:
	void load(const std::filesystem::path &path);

	template <typename... TArgs>
	[[nodiscard]] std::string translate(
		std::string_view identifier,
		TArgs &&...args) const
	{
		return _translate(
			identifier,
			std::make_format_args(args...));
	}
};
