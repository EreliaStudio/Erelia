#pragma once

#include <cstddef>
#include <vector>

#include <container/thread_safe_set.hpp>

#include "erelia/core/chunk_collection.hpp"
#include "erelia/core/task_group.hpp"

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

	struct PendingTaskGroup
	{
		std::vector<Chunk::Collection::Request> requests;
		spk::TaskGroup<Chunk>::Answer answer;
	};

	RequestSet _requested;
	std::vector<PendingTaskGroup> _pending;

public:
	PrototypeChunkProvider() = default;
	PrototypeChunkProvider(const PrototypeChunkProvider &) = delete;
	PrototypeChunkProvider(PrototypeChunkProvider &&) noexcept = default;

	PrototypeChunkProvider &operator=(const PrototypeChunkProvider &) = delete;
	PrototypeChunkProvider &operator=(PrototypeChunkProvider &&) noexcept = default;

	void request(const Chunk::Collection::Request &request) override;
	void update(Chunk::Collection &collection) override;
};
