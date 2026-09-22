#pragma once

#include <limits>
#include <set>
#include <unordered_map>

#include "voxel/chunk_coordinate_hash.hpp"
#include "voxel/chunk_mesher.hpp"

namespace voxel
{
	class Chunk::BakeScheduler final
	{
	public:
		using BakeCompletionProvider = spk::ContractProvider<Coordinate, const spk::TextureMesh3D &>;

	private:
		Collection &_chunks;
		const Mesher &_mesher;
		spk::Profiler *_profiler = nullptr;
		BakeCompletionProvider _bakeCompletions;
		std::set<Coordinate> _dirty;
		Collection::ChunkProvider::Contract _available;
		Collection::CoordinateProvider::Contract _removed;
		std::unordered_map<Coordinate, spk::VersionedTrait::Contract, CoordinateHash> _versions;

		void _observe(Chunk &chunk);
		void _invalidateNeighbors(Coordinate coordinate);

	public:
		BakeScheduler(Collection &chunks, const Mesher &mesher, spk::Profiler *profiler = nullptr);

		[[nodiscard]] std::size_t pendingCount() const noexcept
		{
			return _dirty.size();
		}

		void invalidate(Coordinate coordinate);
		std::size_t process(std::size_t budget = std::numeric_limits<std::size_t>::max());

		[[nodiscard]] BakeCompletionProvider::Contract subscribeToBakeCompletion(BakeCompletionProvider::callback_type callback);
	};
}
