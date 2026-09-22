#pragma once

#include <memory>
#include <optional>
#include <set>
#include <unordered_map>

#include "voxel/chunk.hpp"
#include "voxel/chunk_coordinate_hash.hpp"

namespace voxel
{
	class Chunk::Collection final
	{
	public:
		using CoordinateProvider = spk::ContractProvider<Coordinate>;
		using ChunkProvider = spk::ContractProvider<Chunk &>;
		enum class State
		{
			Absent,
			Pending,
			Available
		};

	private:
		struct Entry
		{
			std::unique_ptr<Chunk> chunk;
		};

		std::unordered_map<Coordinate, Entry, CoordinateHash> _chunks;
		std::set<Coordinate> _pending;
		CoordinateProvider _requests;
		CoordinateProvider _removed;
		ChunkProvider _available;

	public:
		[[nodiscard]] State state(Coordinate coordinate) const noexcept;
		[[nodiscard]] Chunk *find(Coordinate coordinate) noexcept;
		[[nodiscard]] const Chunk *find(Coordinate coordinate) const noexcept;
		[[nodiscard]] std::optional<Voxel::Cell> worldCell(spk::Vector3Int world) const;

		bool request(Coordinate coordinate);
		void fail(Coordinate coordinate);
		void publish(std::unique_ptr<Chunk> chunk);
		bool remove(Coordinate coordinate);

		[[nodiscard]] CoordinateProvider::Contract subscribeToRequests(CoordinateProvider::callback_type callback);
		[[nodiscard]] ChunkProvider::Contract subscribeToAvailability(ChunkProvider::callback_type callback);
		[[nodiscard]] CoordinateProvider::Contract subscribeToRemoval(CoordinateProvider::callback_type callback);
	};
}
