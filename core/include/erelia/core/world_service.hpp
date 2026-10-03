#pragma once

#include "erelia/core/world.hpp"
#include "erelia/core/world_identifier.hpp"

#include <concepts>
#include <cstddef>
#include <memory>
#include <type_traits>
#include <unordered_map>
#include <utility>

#include <exception.hpp>

class WorldProvider
{
	friend class WorldService;

protected:
	[[nodiscard]] virtual std::unique_ptr<World> _acquire(
		const WorldIdentifier &identifier) = 0;

public:
	WorldProvider() = default;
	WorldProvider(WorldProvider &&) noexcept = default;
	WorldProvider(const WorldProvider &) = delete;
	virtual ~WorldProvider() = default;
};

class WorldService
{
private:
	std::unordered_map<WorldIdentifier, std::unique_ptr<World>> _worlds;
	std::unique_ptr<WorldProvider> _provider;

public:
	template <typename TProvider>
		requires std::derived_from<std::remove_cvref_t<TProvider>, WorldProvider> &&
				 (std::is_lvalue_reference_v<TProvider> == false)
	explicit WorldService(TProvider &&provider) :
		_provider(
			std::make_unique<std::remove_cvref_t<TProvider>>(
				std::forward<TProvider>(provider)))
	{
	}

	WorldService(const WorldService &) = delete;
	WorldService &operator=(const WorldService &) = delete;
	WorldService(WorldService &&) = delete;
	WorldService &operator=(WorldService &&) = delete;

	[[nodiscard]] World *world(
		const WorldIdentifier &identifier)
	{
		auto found = _worlds.find(identifier);
		if (found != _worlds.end())
		{
			return found->second.get();
		}

		std::unique_ptr<World> acquired =
			_provider->_acquire(identifier);
		if (acquired == nullptr)
		{
			throw spk::Exception(
				"WorldProvider returned no World");
		}
		if (acquired->identifier() != identifier)
		{
			throw spk::Exception(
				"WorldProvider returned a mismatched World identifier");
		}

		World *result = acquired.get();
		_worlds.emplace(
			identifier,
			std::move(acquired));
		return result;
	}

	[[nodiscard]] World *find(
		const WorldIdentifier &identifier) noexcept
	{
		auto found = _worlds.find(identifier);
		return found == _worlds.end()
				   ? nullptr
				   : found->second.get();
	}

	[[nodiscard]] const World *find(
		const WorldIdentifier &identifier) const noexcept
	{
		auto found = _worlds.find(identifier);
		return found == _worlds.end()
				   ? nullptr
				   : found->second.get();
	}

	[[nodiscard]] bool contains(
		const WorldIdentifier &identifier) const noexcept
	{
		return _worlds.contains(identifier);
	}

	[[nodiscard]] bool remove(
		const WorldIdentifier &identifier)
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

	[[nodiscard]] WorldProvider &provider() noexcept
	{
		return *_provider;
	}

	[[nodiscard]] const WorldProvider &provider() const noexcept
	{
		return *_provider;
	}
};
