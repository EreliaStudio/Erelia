#pragma once
#include "erelia/core/world.hpp"
#include "erelia/core/player_information.hpp"
#include <atomic>
#include <container/thread_safe_fifo.hpp>
#include <engine/behaviour3d.hpp>
#include <engine/transform3d.hpp>
#include <unordered_set>
class TerrainStreamingBehaviour final : public spk::Behaviour3D
{
public:
	struct Ranges
	{
		std::int32_t viewRange;
		std::int32_t unloadRange;
		void validate() const;
	};

private:
	using ColumnAnswer = spk::Task<Column>::Answer;
	using ChunkAnswer = spk::Task<Chunk>::Answer;
	struct ColumnCompletion
	{
		Column::Coordinate key;
		ColumnAnswer answer;
	};
	struct ChunkCompletion
	{
		Chunk::Coordinate key;
		ChunkAnswer answer;
	};
	struct ColumnSubscription
	{
		ColumnAnswer answer;
		ColumnAnswer::CompletionContract contract;
		std::shared_ptr<std::atomic_bool> published;
	};
	struct ChunkSubscription
	{
		ChunkAnswer answer;
		ChunkAnswer::CompletionContract contract;
		std::shared_ptr<std::atomic_bool> published;
	};
	World::Columns &_columns;
	World::Chunks &_chunks;
	Ranges _ranges;
	std::vector<Column::Coordinate> _viewOffsets;
	std::unordered_set<Column::Coordinate> _viewRegion;
	std::unordered_set<Column::Coordinate> _unloadRegion;
	std::optional<Chunk::Coordinate> _center;
	spk::Transform3D::OnEditionContract _transformContract;
	spk::ThreadSafeFIFO<ColumnCompletion> _columnCompletions;
	spk::ThreadSafeFIFO<ChunkCompletion> _chunkCompletions;
	std::vector<ColumnCompletion> _drainedColumns;
	std::vector<ChunkCompletion> _drainedChunks;
	std::vector<ColumnSubscription> _columnSubscriptions;
	std::vector<ChunkSubscription> _chunkSubscriptions;
	void _refresh(const spk::Transform3D &transform);
	void _requestColumns(const std::vector<Column::Coordinate> &keys);
	void _requestChunks(const Column &column);
	[[nodiscard]] bool _inside(Column::Coordinate coordinate, std::int32_t range) const;
	void _updateState(spk::UpdateContext &) override;

public:
	TerrainStreamingBehaviour(World::Columns &columns, World::Chunks &chunks, Ranges ranges);
	void attach(spk::Entity *owner) override;
	void dispatch();
	[[nodiscard]] std::optional<Chunk::Coordinate> center() const noexcept
	{
		return _center;
	}
};
class Player final : public spk::Entity3D
{
public:
	Player(
		const PlayerInformation &information,
		World &world,
		TerrainStreamingBehaviour::Ranges ranges) :
		spk::Entity3D("Player")
	{
		(void)information;
		addBehaviour<TerrainStreamingBehaviour>(
			*world.columnCollection(),
			*world.chunkCollection(),
			ranges);
		activate();
	}
};
