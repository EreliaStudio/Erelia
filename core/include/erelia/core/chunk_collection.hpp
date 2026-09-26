#pragma once

#include <concepts>
#include <cstddef>
#include <cstdint>
#include <exception>
#include <memory>
#include <optional>
#include <type_traits>
#include <unordered_map>
#include <utility>
#include <vector>

#include <container/protected_data.hpp>
#include <threading/task.hpp>

#include "erelia/core/chunk.hpp"

class Chunk::Collection final
{
public:
	enum class State
	{
		Absent,
		Pending,
		Available
	};

	struct BatchResult final
	{
		struct Acquired final
		{
			Chunk::Coordinate coordinate;
			Chunk chunk;
		};

		struct Failed final
		{
			Chunk::Coordinate coordinate;
			std::exception_ptr exception;
		};

		std::vector<Acquired> acquired;
		std::vector<Failed> failed;
	};

	class Provider
	{
	public:
		virtual ~Provider() = default;

		[[nodiscard]] virtual spk::Task<Chunk>::Answer request(
			const Chunk::Coordinate &coordinate) = 0;
	};

private:
	using Generation = std::uint64_t;
	using ChunkAnswer = spk::Task<Chunk>::Answer;

	struct Entry final
	{
		Generation generation = 0u;
		std::optional<Chunk> chunk;
		std::optional<ChunkAnswer> pending;
	};

	struct Storage final
	{
		std::unordered_map<Chunk::Coordinate, Entry> chunks;
		Generation nextGeneration = 1u;
	};

	std::unique_ptr<Provider> _provider;
	std::shared_ptr<spk::ProtectedData<Storage>> _storage;

public:
	template <typename TProvider>
		requires std::derived_from<std::remove_cvref_t<TProvider>, Provider> &&
				 (!std::is_lvalue_reference_v<TProvider>)
	explicit Collection(TProvider &&provider) :
		_provider(
			std::make_unique<
				std::remove_cvref_t<TProvider>>(
				std::forward<TProvider>(provider))),
		_storage(
			std::make_shared<
				spk::ProtectedData<Storage>>())
	{
	}

	[[nodiscard]] State state(
		const Chunk::Coordinate &coordinate) const;
	[[nodiscard]] std::optional<Chunk> tryGet(
		const Chunk::Coordinate &coordinate) const;

	[[nodiscard]] spk::Task<BatchResult>::Answer request(
		const std::vector<Chunk::Coordinate> &coordinates);

	void replace(
		const Chunk::Coordinate &coordinate,
		Chunk chunk);
};
