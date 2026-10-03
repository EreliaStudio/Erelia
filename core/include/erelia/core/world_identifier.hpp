#pragma once

#include <compare>
#include <cstddef>
#include <functional>
#include <string>

struct WorldIdentifier
{
	std::string name;

	auto operator<=>(const WorldIdentifier &) const = default;
};

namespace std
{
	template <>
	struct hash<WorldIdentifier>
	{
		size_t operator()(const WorldIdentifier &identifier) const noexcept
		{
			return hash<std::string>{}(identifier.name);
		}
	};
}
