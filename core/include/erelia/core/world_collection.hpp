#pragma once

#include "erelia/core/world.hpp"

#include <cstddef>
#include <memory>
#include <string>
#include <unordered_map>
#include <utility>

class WorldCollection
{
public:
	using Identifier = std::string;

private:
	std::unordered_map<Identifier, std::unique_ptr<World>> _worlds;

	[[nodiscard]] virtual std::unique_ptr<World> _createWorld(
		const Identifier &identifier) = 0;

public:
	virtual ~WorldCollection() = default;

	WorldCollection() = default;
	WorldCollection(const WorldCollection &) = delete;
	WorldCollection &operator=(const WorldCollection &) = delete;
	WorldCollection(WorldCollection &&) = delete;
	WorldCollection &operator=(WorldCollection &&) = delete;

	[[nodiscard]] World *world(
		const Identifier &identifier)
	{
		auto found = _worlds.find(identifier);
		if (found != _worlds.end())
		{
			return found->second.get();
		}

		std::unique_ptr<World> created =
			_createWorld(identifier);
		World *result = created.get();
		_worlds.emplace(
			identifier,
			std::move(created));
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

	void clear()
	{
		_worlds.clear();
	}

	[[nodiscard]] std::size_t size() const noexcept
	{
		return _worlds.size();
	}
};
