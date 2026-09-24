#pragma once

#include <concepts>
#include <cstdint>
#include <memory>
#include <optional>
#include <type_traits>
#include <unordered_map>
#include <utility>

#include <container/protected_data.hpp>

#include "erelia/core/chunk.hpp"

class Chunk::Collection final
{
public:
	using Generation = std::uint64_t;

	enum class State
	{
		Absent,
		Pending,
		Available
	};

	struct Request
	{
		Chunk::Coordinate coordinate;
		Generation generation = 0u;

		[[nodiscard]] bool operator==(const Request &) const noexcept = default;
	};

	class Provider
	{
	public:
		virtual ~Provider() = default;

		virtual void request(const Request &request) = 0;
		virtual void update(Collection &collection) = 0;
	};

private:
	struct Entry
	{
		Generation generation = 0u;
		std::optional<Chunk> chunk;
	};

	struct Storage
	{
		std::unordered_map<Chunk::Coordinate, Entry> chunks;
		Generation nextGeneration = 1u;
	};

	std::unique_ptr<Provider> _provider;
	spk::ProtectedData<Storage> _storage;

public:
	template <typename TProvider>
		requires std::derived_from<std::remove_cvref_t<TProvider>, Provider> &&
				 (!std::is_lvalue_reference_v<TProvider>)
	explicit Collection(TProvider &&provider) :
		_provider(
			std::make_unique<std::remove_cvref_t<TProvider>>(
				std::forward<TProvider>(provider)))
	{
	}

	[[nodiscard]] State state(const Chunk::Coordinate &coordinate) const;
	[[nodiscard]] std::optional<Chunk> tryGet(
		const Chunk::Coordinate &coordinate) const;

	[[nodiscard]] bool request(const Chunk::Coordinate &coordinate);
	void update();

	[[nodiscard]] bool isPending(const Request &request) const;
	[[nodiscard]] bool publish(const Request &request, Chunk chunk);
	[[nodiscard]] bool fail(const Request &request);

	void replace(const Chunk::Coordinate &coordinate, Chunk chunk);
};
