#include "voxel/chunk_collection.hpp"

#include <utility>

#include <exception.hpp>

namespace voxel
{
	bool Chunk::Collection::request(Coordinate coordinate)
	{
		if (state(coordinate) != State::Absent)
		{
			return false;
		}
		_pending.insert(coordinate);
		try
		{
			_requests.trigger(coordinate);
		} catch (...)
		{
			_pending.erase(coordinate);
			throw;
		}
		return true;
	}

	void Chunk::Collection::fail(Coordinate coordinate)
	{
		_pending.erase(coordinate);
	}

	void Chunk::Collection::publish(std::unique_ptr<Chunk> chunk)
	{
		if (!chunk)
		{
			throw spk::Exception("cannot publish a null chunk");
		}
		const auto coordinate = chunk->coordinate();
		if (_chunks.contains(coordinate))
		{
			throw spk::Exception("duplicate chunk publication");
		}
		if (!_pending.erase(coordinate))
		{
			throw spk::Exception("published chunk was not pending");
		}
		Entry entry;
		entry.chunk = std::move(chunk);
		auto [iterator, inserted] = _chunks.emplace(coordinate, std::move(entry));
		(void)inserted;
		_available.trigger(*iterator->second.chunk);
	}

	bool Chunk::Collection::remove(Coordinate coordinate)
	{
		_pending.erase(coordinate);
		auto iterator = _chunks.find(coordinate);
		if (iterator == _chunks.end())
		{
			return false;
		}
		_chunks.erase(iterator);
		_removed.trigger(coordinate);
		return true;
	}

	Chunk::Collection::State Chunk::Collection::state(Coordinate coordinate) const noexcept
	{
		if (_chunks.contains(coordinate))
		{
			return State::Available;
		}
		if (_pending.contains(coordinate))
		{
			return State::Pending;
		}
		return State::Absent;
	}

	Chunk *Chunk::Collection::find(Coordinate coordinate) noexcept
	{
		auto iterator = _chunks.find(coordinate);
		return iterator == _chunks.end() ? nullptr : iterator->second.chunk.get();
	}

	const Chunk *Chunk::Collection::find(Coordinate coordinate) const noexcept
	{
		return const_cast<Collection *>(this)->find(coordinate);
	}

	std::optional<Voxel::Cell> Chunk::Collection::worldCell(spk::Vector3Int world) const
	{
		auto *chunk = find(Chunk::worldToChunk(world));
		if (!chunk)
		{
			return std::nullopt;
		}
		return chunk->at(Chunk::worldToLocal(world));
	}

	Chunk::Collection::CoordinateProvider::Contract Chunk::Collection::subscribeToRequests(CoordinateProvider::callback_type callback)
	{
		return _requests.subscribe(std::move(callback));
	}
	Chunk::Collection::ChunkProvider::Contract Chunk::Collection::subscribeToAvailability(ChunkProvider::callback_type callback)
	{
		return _available.subscribe(std::move(callback));
	}
	Chunk::Collection::CoordinateProvider::Contract Chunk::Collection::subscribeToRemoval(CoordinateProvider::callback_type callback)
	{
		return _removed.subscribe(std::move(callback));
	}
}
