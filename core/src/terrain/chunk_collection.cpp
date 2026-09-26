#include "erelia/core/chunk_collection.hpp"

#include <cstddef>
#include <exception>
#include <memory>
#include <mutex>
#include <optional>
#include <utility>
#include <vector>

namespace
{
	using Collection = Chunk::Collection;
	using BatchResult = Collection::BatchResult;
	using BatchTask = spk::Task<BatchResult>;
	using ChunkTask = spk::Task<Chunk>;
	using CompletionContract =
		typename ChunkTask::Answer::CompletionContract;

	struct BatchState final
	{
		std::mutex mutex;
		BatchTask task;
		BatchResult result;
		std::vector<CompletionContract> contracts;
		std::size_t remaining = 0u;
		bool settled = false;

		explicit BatchState(std::size_t count) :
			remaining(count)
		{
		}

		[[nodiscard]] BatchTask::Answer answer() const
		{
			return task.answer();
		}

		void addContract(CompletionContract contract)
		{
			const std::scoped_lock lock(mutex);
			if (!settled)
			{
				contracts.push_back(std::move(contract));
			}
		}

		void acquired(
			const Chunk::Coordinate &coordinate,
			const Chunk &chunk)
		{
			std::optional<BatchResult> completed;
			std::exception_ptr failure;

			{
				const std::scoped_lock lock(mutex);
				if (settled)
				{
					return;
				}

				try
				{
					result.acquired.push_back(
						{coordinate, chunk});
				} catch (...)
				{
					settled = true;
					failure = std::current_exception();
				}

				if (failure == nullptr)
				{
					--remaining;
					if (remaining == 0u)
					{
						settled = true;
						completed.emplace(
							std::move(result));
					}
				}
			}

			if (failure != nullptr)
			{
				task.fail(std::move(failure));
			}
			else if (completed.has_value())
			{
				task.validate(
					std::move(*completed));
			}
		}

		void failed(
			const Chunk::Coordinate &coordinate,
			std::exception_ptr exception)
		{
			std::optional<BatchResult> completed;
			std::exception_ptr aggregationFailure;

			{
				const std::scoped_lock lock(mutex);
				if (settled)
				{
					return;
				}

				try
				{
					result.failed.push_back(
						{coordinate, std::move(exception)});
				} catch (...)
				{
					settled = true;
					aggregationFailure =
						std::current_exception();
				}

				if (aggregationFailure == nullptr)
				{
					--remaining;
					if (remaining == 0u)
					{
						settled = true;
						completed.emplace(
							std::move(result));
					}
				}
			}

			if (aggregationFailure != nullptr)
			{
				task.fail(
					std::move(aggregationFailure));
			}
			else if (completed.has_value())
			{
				task.validate(
					std::move(*completed));
			}
		}

		void abort(std::exception_ptr exception)
		{
			bool shouldFail = false;
			{
				const std::scoped_lock lock(mutex);
				if (!settled)
				{
					settled = true;
					shouldFail = true;
				}
			}

			if (shouldFail)
			{
				task.fail(std::move(exception));
			}
		}

		void completeEmpty()
		{
			std::optional<BatchResult> completed;
			{
				const std::scoped_lock lock(mutex);
				if (!settled && remaining == 0u)
				{
					settled = true;
					completed.emplace(
						std::move(result));
				}
			}

			if (completed.has_value())
			{
				task.validate(
					std::move(*completed));
			}
		}
	};
}

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
		std::make_shared<BatchState>(
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

			const ChunkAnswer answer = *pending;
			auto contract =
				answer.subscribeToCompletion(
					[batch,
					 weakStorage,
					 coordinate,
					 generation,
					 answer] {
						try
						{
							if (
								answer.status() ==
								ChunkTask::Status::Completed)
							{
								if (
									const auto storage =
										weakStorage.lock();
									storage != nullptr)
								{
									auto writer =
										storage->write();
									const auto found =
										writer->chunks.find(
											coordinate);
									if (
										found !=
											writer->chunks.end() &&
										found->second.generation ==
											generation &&
										found->second.pending.has_value())
									{
										found->second.chunk =
											answer.result();
										found->second.pending.reset();
									}
								}

								batch->acquired(
									coordinate,
									answer.result());
							}
							else
							{
								if (
									const auto storage =
										weakStorage.lock();
									storage != nullptr)
								{
									auto writer =
										storage->write();
									const auto found =
										writer->chunks.find(
											coordinate);
									if (
										found !=
											writer->chunks.end() &&
										found->second.generation ==
											generation &&
										found->second.pending.has_value())
									{
										writer->chunks.erase(
											found);
									}
								}

								batch->failed(
									coordinate,
									answer.failure());
							}
						} catch (...)
						{
							batch->abort(
								std::current_exception());
						}
					});

			batch->addContract(
				std::move(contract));
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
