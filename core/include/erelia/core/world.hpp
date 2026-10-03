#pragma once

#include "erelia/core/collection.hpp"
#include "erelia/core/column.hpp"

#include <concepts>
#include <memory>
#include <utility>
#include <vector>

#include <engine/engine.hpp>
#include <engine/entity.hpp>

class World
{
public:
	using Chunks = Collection<Chunk::Coordinate, Chunk>;
	using Columns = Collection<Column::Coordinate, Column>;

private:
	std::unique_ptr<Chunks> _chunks;
	std::unique_ptr<Columns> _columns;
	std::vector<std::unique_ptr<spk::Entity>> _entities;
	spk::Engine _engine;

	[[nodiscard]] virtual std::unique_ptr<Chunks> _createChunkCollection() = 0;
	[[nodiscard]] virtual std::unique_ptr<Columns> _createColumnCollection() = 0;

protected:
	[[nodiscard]] Chunks *_existingChunkCollection() noexcept
	{
		return _chunks.get();
	}

	[[nodiscard]] Columns *_existingColumnCollection() noexcept
	{
		return _columns.get();
	}

public:
	World() = default;
	virtual ~World() = default;

	World(const World &) = delete;
	World &operator=(const World &) = delete;
	World(World &&) = delete;
	World &operator=(World &&) = delete;

	[[nodiscard]] Chunks *chunkCollection()
	{
		if (_chunks == nullptr)
		{
			_chunks = _createChunkCollection();
		}
		return _chunks.get();
	}

	[[nodiscard]] Columns *columnCollection()
	{
		if (_columns == nullptr)
		{
			_columns = _createColumnCollection();
		}
		return _columns.get();
	}

	[[nodiscard]] spk::Engine &engine() noexcept
	{
		return _engine;
	}

	[[nodiscard]] const spk::Engine &engine() const noexcept
	{
		return _engine;
	}

	template <typename TEntity, typename... TArguments>
		requires std::derived_from<TEntity, spk::Entity>
	[[nodiscard]] TEntity *addEntity(TArguments &&...arguments)
	{
		auto entity = std::make_unique<TEntity>(
			std::forward<TArguments>(arguments)...);
		TEntity *result = entity.get();
		_engine.addEntity(result);
		_entities.push_back(std::move(entity));
		return result;
	}
};
