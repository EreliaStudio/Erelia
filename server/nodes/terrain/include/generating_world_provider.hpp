#pragma once

#include "erelia/core/world_collection.hpp"

#include <cstdint>
#include <string>
#include <unordered_map>

class GeneratingWorldProvider final : public WorldCollection::Provider
{
public:
	enum class Type : std::uint8_t
	{
		Prototype
	};

	struct Definition
	{
		WorldIdentifier identifier;
		Type generatorType;
		std::string family;
	};

	inline static const WorldIdentifier PrototypeWorld{
		.name = "prototype"};

private:
	std::unordered_map<WorldIdentifier, Definition> _definitions;

protected:
	[[nodiscard]] std::unique_ptr<World> _acquire(
		const WorldIdentifier &identifier) override;

public:
	void define(Definition definition);

	[[nodiscard]] bool contains(
		const WorldIdentifier &identifier) const noexcept;
};
