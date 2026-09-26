#include "erelia/core/chunk_collection.hpp"

#include "chunk_collection_acquisition.hpp"
#include "chunk_collection_batch.hpp"

#include <exception>
#include <memory>
#include <optional>
#include <utility>
#include <vector>

Chunk::Collection::State Chunk::Collection::state(
	const Chunk::Coordinate &coordinate) const
{
	auto reader = _storage->read();
	const auto found =
		reader->chunks.find(coordinate);
	if (found == reader->chunks.end())
	{
		return State::Absent;
	}
	return found->second.chunk.has_value() ? State::Available : State::Pending;
}

std::optional<Chunk> Chunk::Collection::tryGet(
	const Chunk::Coordinate &coordinate) const
{
	auto reader = _storage->read();
	const auto found =
		reader->chunks.find(coordinate);
	if (
		found == reader->chunks.end() ||
		!found->second.chunk.has_value())
	{
		return std::nullopt;
	}
	return found->second.chunk;
}

spk::Task<Chunk::Collection::BatchResult>::Answer
Chunk::Collection::request(
	const std::vector<Chunk::Coordinate> &coordinates)
{
	auto batch =
		std::make_shared<Batch>(
			coordinates.size());
	const auto batchAnswer = batch->answer();

	if (coordinates.empty())
	{
		batch->completeEmpty();
		return batchAnswer;
	}

	const std::weak_ptr<spk::ProtectedData<Storage>> weakStorage = _storage;

	for (const Coordinate &coordinate : coordinates)
	{
		try
		{
			std::optional<Chunk> available;
			std::optional<ChunkAnswer> pending;
			Generation generation = 0u;
			std::exception_ptr immediateFailure;

			{
				auto writer = _storage->write();
				auto found =
					writer->chunks.find(coordinate);

				if (found != writer->chunks.end())
				{
					generation =
						found->second.generation;
					if (found->second.chunk.has_value())
					{
						available =
							found->second.chunk;
					}
					else
					{
						pending =
							found->second.pending;
					}
				}
				else
				{
					generation =
						writer->nextGeneration++;
					try
					{
						ChunkAnswer answer =
							_provider->request(
								coordinate);
						writer->chunks.emplace(
							coordinate,
							Entry{
								.generation =
									generation,
								.chunk =
									std::nullopt,
								.pending =
									answer});
						pending =
							std::move(answer);
					} catch (...)
					{
						immediateFailure =
							std::current_exception();
					}
				}
			}

			if (available.has_value())
			{
				batch->acquired(
					coordinate,
					*available);
				continue;
			}

			if (immediateFailure != nullptr)
			{
				batch->failed(
					coordinate,
					std::move(immediateFailure));
				continue;
			}

			if (!pending.has_value())
			{
				batch->abort(
					std::make_exception_ptr(
						spk::Exception(
							"Chunk::Collection Pending entry has no Task Answer")));
				continue;
			}

			Acquisition acquisition(
				coordinate,
				generation,
				*pending,
				weakStorage,
				batch);
			batch->addContract(
				acquisition.subscribe());
		} catch (...)
		{
			batch->abort(
				std::current_exception());
		}
	}

	return batchAnswer;
}

void Chunk::Collection::replace(
	const Chunk::Coordinate &coordinate,
	Chunk chunk)
{
	auto writer = _storage->write();
	const Generation generation =
		writer->nextGeneration++;
	writer->chunks.insert_or_assign(
		coordinate,
		Entry{
			.generation = generation,
			.chunk = std::move(chunk),
			.pending = std::nullopt});
}
