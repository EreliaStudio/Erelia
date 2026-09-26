#pragma once

#include "erelia/core/chunk_collection.hpp"

#include <memory>

class Chunk::Collection::Acquisition final
{
private:
	Coordinate _coordinate;
	Generation _generation;
	ChunkAnswer _answer;
	std::weak_ptr<spk::ProtectedData<Storage>> _storage;
	std::shared_ptr<Batch> _batch;

	void _complete() const;
	void _publishAcquired() const;
	void _discardFailed() const;

public:
	Acquisition(
		Coordinate coordinate,
		Generation generation,
		ChunkAnswer answer,
		std::weak_ptr<spk::ProtectedData<Storage>> storage,
		std::shared_ptr<Batch> batch);

	[[nodiscard]] ChunkAnswer::CompletionContract
	subscribe() const;
};
