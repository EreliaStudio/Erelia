#pragma once

#include <cstddef>
#include <vector>

#include <container/thread_safe_set.hpp>
#include <threading/task.hpp>

#include "erelia/core/chunk_collection.hpp"

class PrototypeChunkProvider final : public Chunk::Collection::Provider
{
private:
	struct RequestHash
	{
		[[nodiscard]] std::size_t operator()(
			const Chunk::Collection::Request &request) const noexcept;
	};

	using RequestSet = spk::ThreadSafeSet<
		Chunk::Collection::Request,
		RequestHash>;

	struct PendingTask
	{
		Chunk::Collection::Request request;
		spk::Task<Chunk>::Answer answer;
	};

	RequestSet _requested;
	std::vector<PendingTask> _pending;

public:
	PrototypeChunkProvider() = default;
	PrototypeChunkProvider(const PrototypeChunkProvider &) = delete;
	PrototypeChunkProvider(PrototypeChunkProvider &&) noexcept = default;

	PrototypeChunkProvider &operator=(const PrototypeChunkProvider &) = delete;
	PrototypeChunkProvider &operator=(PrototypeChunkProvider &&) noexcept = default;

	void request(const Chunk::Collection::Request &request) override;
	void update(Chunk::Collection &collection) override;
};
