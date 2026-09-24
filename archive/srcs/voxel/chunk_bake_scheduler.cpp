#include "voxel/chunk_bake_scheduler.hpp"

#include <utility>
#include <memory>

namespace voxel
{
	Chunk::BakeScheduler::BakeScheduler(Collection &chunks, const Mesher &mesher, spk::Profiler *profiler) :
		_chunks(chunks),
		_mesher(mesher),
		_profiler(profiler),
		_available(chunks.subscribeToAvailability([this](Chunk &chunk) {
			_observe(chunk);
			invalidate(chunk.coordinate());
			_invalidateNeighbors(chunk.coordinate());
		})),
		_removed(chunks.subscribeToRemoval([this](Coordinate coordinate) {
			_versions.erase(coordinate);
			_dirty.erase(coordinate);
			_invalidateNeighbors(coordinate);
		}))
	{
	}

	void Chunk::BakeScheduler::_observe(Chunk &chunk)
	{
		const Coordinate coordinate = chunk.coordinate();
		_versions.insert_or_assign(coordinate, chunk.subscribeToVersionEdition([this, coordinate](spk::VersionedTrait *) {
			invalidate(coordinate);
			_invalidateNeighbors(coordinate);
		}));
	}

	void Chunk::BakeScheduler::invalidate(Coordinate coordinate)
	{
		if (_chunks.find(coordinate))
		{
			_dirty.insert(coordinate);
		}
	}

	void Chunk::BakeScheduler::_invalidateNeighbors(Coordinate coordinate)
	{
		invalidate({coordinate.x - 1, coordinate.y, coordinate.z});
		invalidate({coordinate.x + 1, coordinate.y, coordinate.z});
		invalidate({coordinate.x, coordinate.y - 1, coordinate.z});
		invalidate({coordinate.x, coordinate.y + 1, coordinate.z});
		invalidate({coordinate.x, coordinate.y, coordinate.z - 1});
		invalidate({coordinate.x, coordinate.y, coordinate.z + 1});
	}

	std::size_t Chunk::BakeScheduler::process(std::size_t budget)
	{
		std::unique_ptr<spk::Profiler::TimeMeasurement::Scope> group;
		if (_profiler != nullptr)
			group = std::make_unique<spk::Profiler::TimeMeasurement::Scope>(_profiler->timeMeasurement("Chunks/bake group"));
		std::size_t count = 0;
		while (count < budget && !_dirty.empty())
		{
			const auto coordinate = *_dirty.begin();
			_dirty.erase(_dirty.begin());
			if (auto *chunk = _chunks.find(coordinate))
			{
				std::unique_ptr<spk::Profiler::TimeMeasurement::Scope> chunkTimer;
				if (_profiler != nullptr)
					chunkTimer = std::make_unique<spk::Profiler::TimeMeasurement::Scope>(_profiler->timeMeasurement("Chunks/bake chunk"));
				const spk::TextureMesh3D mesh = _mesher.bake(*chunk);
				_bakeCompletions.trigger(coordinate, mesh);
				++count;
			}
		}
		return count;
	}

	Chunk::BakeScheduler::BakeCompletionProvider::Contract Chunk::BakeScheduler::subscribeToBakeCompletion(BakeCompletionProvider::callback_type callback)
	{
		return _bakeCompletions.subscribe(std::move(callback));
	}
}
