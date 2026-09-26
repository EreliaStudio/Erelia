#include "chunk_collection_acquisition.hpp"

#include "chunk_collection_batch.hpp"

#include <exception>
#include <utility>

Chunk::Collection::Acquisition::Acquisition(
	Coordinate coordinate,
	Generation generation,
	ChunkAnswer answer,
	std::weak_ptr<spk::ProtectedData<Storage>> storage,
	std::shared_ptr<Batch> batch) :
	_coordinate(coordinate),
	_generation(generation),
	_answer(std::move(answer)),
	_storage(std::move(storage)),
	_batch(std::move(batch))
{
}

Chunk::Collection::ChunkAnswer::CompletionContract
Chunk::Collection::Acquisition::subscribe() const
{
	return _answer.subscribeToCompletion(
		[acquisition = *this] {
			acquisition._complete();
		});
}

void Chunk::Collection::Acquisition::_complete() const
{
	try
	{
		if (
			_answer.status() ==
			spk::Task<Chunk>::Status::Completed)
		{
			_publishAcquired();
			_batch->acquired(
				_coordinate,
				_answer.result());
		}
		else
		{
			_discardFailed();
			_batch->failed(
				_coordinate,
				_answer.failure());
		}
	} catch (...)
	{
		_batch->abort(
			std::current_exception());
	}
}

void Chunk::Collection::Acquisition::_publishAcquired() const
{
	const auto storage = _storage.lock();
	if (storage == nullptr)
	{
		return;
	}

	auto writer = storage->write();
	const auto found =
		writer->chunks.find(_coordinate);
	if (
		found != writer->chunks.end() &&
		found->second.generation == _generation &&
		found->second.pending.has_value() == true)
	{
		found->second.chunk =
			_answer.result();
		found->second.pending.reset();
	}
}

void Chunk::Collection::Acquisition::_discardFailed() const
{
	const auto storage = _storage.lock();
	if (storage == nullptr)
	{
		return;
	}

	auto writer = storage->write();
	const auto found =
		writer->chunks.find(_coordinate);
	if (
		found != writer->chunks.end() &&
		found->second.generation == _generation &&
		found->second.pending.has_value() == true)
	{
		writer->chunks.erase(found);
	}
}
