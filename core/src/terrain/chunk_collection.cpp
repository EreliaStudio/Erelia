#include "erelia/core/chunk_collection.hpp"

#include <utility>

Chunk Chunk::Collection::get(const Chunk::Coordinate &coordinate)
{
	std::scoped_lock lock(_mutex);

	const auto found = _chunks.find(coordinate);
	if (found != _chunks.end())
	{
		return found->second;
	}

	Chunk provided = _provider->provide(coordinate);
	const auto [iterator, inserted] =
		_chunks.emplace(coordinate, std::move(provided));
	(void)inserted;
	return iterator->second;
}

void Chunk::Collection::replace(
	const Chunk::Coordinate &coordinate,
	Chunk chunk)
{
	std::scoped_lock lock(_mutex);
	_chunks.insert_or_assign(coordinate, std::move(chunk));
}
