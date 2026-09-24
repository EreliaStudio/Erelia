#include "erelia/core/chunk_collection.hpp"

#include <utility>

Chunk::Collection::State Chunk::Collection::state(
	const Chunk::Coordinate &coordinate) const
{
	auto reader = _storage.read();
	const auto found = reader->chunks.find(coordinate);
	if (found == reader->chunks.end())
	{
		return State::Absent;
	}
	return found->second.chunk.has_value() ? State::Available : State::Pending;
}

std::optional<Chunk> Chunk::Collection::tryGet(
	const Chunk::Coordinate &coordinate) const
{
	auto reader = _storage.read();
	const auto found = reader->chunks.find(coordinate);
	if (found == reader->chunks.end() || !found->second.chunk.has_value())
	{
		return std::nullopt;
	}
	return found->second.chunk;
}

bool Chunk::Collection::request(const Chunk::Coordinate &coordinate)
{
	Request request;
	{
		auto writer = _storage.write();
		if (writer->chunks.contains(coordinate))
		{
			return false;
		}

		request = {
			.coordinate = coordinate,
			.generation = writer->nextGeneration++};
		writer->chunks.emplace(
			coordinate,
			Entry{
				.generation = request.generation,
				.chunk = std::nullopt});
	}

	try
	{
		_provider->request(request);
	} catch (...)
	{
		(void)fail(request);
		throw;
	}
	return true;
}

void Chunk::Collection::update()
{
	_provider->update(*this);
}

bool Chunk::Collection::isPending(const Request &request) const
{
	auto reader = _storage.read();
	const auto found = reader->chunks.find(request.coordinate);
	return found != reader->chunks.end() &&
		   !found->second.chunk.has_value() &&
		   found->second.generation == request.generation;
}

bool Chunk::Collection::publish(const Request &request, Chunk chunk)
{
	auto writer = _storage.write();
	const auto found = writer->chunks.find(request.coordinate);
	if (found == writer->chunks.end() ||
		found->second.chunk.has_value() ||
		found->second.generation != request.generation)
	{
		return false;
	}

	found->second.chunk = std::move(chunk);
	return true;
}

bool Chunk::Collection::fail(const Request &request)
{
	auto writer = _storage.write();
	const auto found = writer->chunks.find(request.coordinate);
	if (found == writer->chunks.end() ||
		found->second.chunk.has_value() ||
		found->second.generation != request.generation)
	{
		return false;
	}

	writer->chunks.erase(found);
	return true;
}

void Chunk::Collection::replace(
	const Chunk::Coordinate &coordinate,
	Chunk chunk)
{
	auto writer = _storage.write();
	writer->chunks.insert_or_assign(
		coordinate,
		Entry{
			.generation = 0u,
			.chunk = std::move(chunk)});
}
