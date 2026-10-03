#pragma once

#include "erelia/core/world.hpp"

#include <concepts>
#include <memory>
#include <string>
#include <unordered_map>
#include <utility>

#include <exception.hpp>

class WorldCollection
{
public:
	using Identifier = std::string;

private:
	std::unordered_map<Identifier, std::unique_ptr<World>> _worlds;

public:
	template <typename TWorld, typename... TArguments>
		requires std::derived_from<TWorld, World>
	[[nodiscard]] TWorld *create(
		Identifier identifier,
		TArguments &&...arguments)
	{
		if (_worlds.contains(identifier) == true)
		{
			throw spk::Exception(
				"World already exists: " + identifier);
		}

		auto world = std::make_unique<TWorld>(
			std::forward<TArguments>(arguments)...);
		TWorld *result = world.get();
		_worlds.emplace(
			std::move(identifier),
			std::move(world));
		return result;
	}

	[[nodiscard]] World *find(
		const Identifier &identifier) noexcept
	{
		auto found = _worlds.find(identifier);
		return found == _worlds.end()
				   ? nullptr
				   : found->second.get();
	}

	[[nodiscard]] const World *find(
		const Identifier &identifier) const noexcept
	{
		auto found = _worlds.find(identifier);
		return found == _worlds.end()
				   ? nullptr
				   : found->second.get();
	}

	[[nodiscard]] bool contains(
		const Identifier &identifier) const noexcept
	{
		return _worlds.contains(identifier);
	}

	[[nodiscard]] bool remove(
		const Identifier &identifier)
	{
		return _worlds.erase(identifier) != 0;
	}

	[[nodiscard]] std::size_t size() const noexcept
	{
		return _worlds.size();
	}
};
