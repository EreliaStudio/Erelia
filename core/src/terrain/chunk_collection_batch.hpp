#pragma once

#include "erelia/core/chunk_collection.hpp"

#include <cstddef>
#include <exception>
#include <mutex>
#include <optional>
#include <vector>

class Chunk::Collection::Batch final
{
private:
	using Task = spk::Task<BatchResult>;
	using CompletionContract =
		ChunkAnswer::CompletionContract;

	std::mutex _mutex;
	Task _task;
	BatchResult _result;
	std::vector<CompletionContract> _contracts;
	std::size_t _remaining = 0u;
	bool _settled = false;

public:
	explicit Batch(std::size_t count);

	[[nodiscard]] Task::Answer answer() const;

	void addContract(CompletionContract contract);

	void acquired(
		const Chunk::Coordinate &coordinate,
		const Chunk &chunk);
	void failed(
		const Chunk::Coordinate &coordinate,
		std::exception_ptr exception);
	void abort(std::exception_ptr exception);
	void completeEmpty();
};
