#pragma once

#include <concepts>
#include <memory>
#include <mutex>
#include <type_traits>
#include <unordered_map>
#include <utility>

#include "erelia/core/chunk.hpp"

class Chunk::Collection final
{
public:
	class Provider
	{
	public:
		virtual ~Provider() = default;

		[[nodiscard]] virtual Chunk provide(
			const Chunk::Coordinate &coordinate) = 0;
	};

private:
	std::unique_ptr<Provider> _provider;
	std::unordered_map<Chunk::Coordinate, Chunk> _chunks;
	std::mutex _mutex;

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

	[[nodiscard]] Chunk get(const Chunk::Coordinate &coordinate);
	void replace(const Chunk::Coordinate &coordinate, Chunk chunk);
};
