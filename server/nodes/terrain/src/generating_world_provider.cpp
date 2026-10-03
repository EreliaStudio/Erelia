#include "generating_world_provider.hpp"

#include "terrain_world.hpp"

#include <exception.hpp>

std::unique_ptr<World> GeneratingWorldProvider::_acquire(
	const WorldIdentifier &identifier)
{
	auto found = _definitions.find(identifier);
	if (found == _definitions.end())
	{
		throw spk::Exception(
			"GeneratingWorldProvider has no definition for World: " +
			identifier.name);
	}

	switch (found->second.generatorType)
	{
	case Type::Prototype:
		return std::make_unique<TerrainWorld>(
			identifier);
	}

	throw spk::Exception(
		"Unsupported World generation type");
}

void GeneratingWorldProvider::define(
	Definition definition)
{
	_definitions.insert_or_assign(
		definition.identifier,
		std::move(definition));
}

bool GeneratingWorldProvider::contains(
	const WorldIdentifier &identifier) const noexcept
{
	return _definitions.contains(identifier);
}
