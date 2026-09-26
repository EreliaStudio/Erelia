#pragma once

#include <threading/task.hpp>

#include "erelia/core/chunk_collection.hpp"

class PrototypeChunkProvider final : public Chunk::Collection::Provider
{
public:
	PrototypeChunkProvider() = default;
	PrototypeChunkProvider(
		const PrototypeChunkProvider &) = delete;
	PrototypeChunkProvider(
		PrototypeChunkProvider &&) noexcept = default;

	PrototypeChunkProvider &operator=(
		const PrototypeChunkProvider &) = delete;
	PrototypeChunkProvider &operator=(
		PrototypeChunkProvider &&) noexcept = default;

	[[nodiscard]] spk::Task<Chunk>::Answer request(
		const Chunk::Coordinate &coordinate) override;
};
