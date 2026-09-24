#pragma once

#include <memory>
#include <unordered_map>

#include "voxel/chunk_collection.hpp"
#include "voxel/chunk_coordinate_hash.hpp"
#include "voxel/chunk_view.hpp"

namespace voxel
{
	class Chunk::ViewCollection final
	{
	private:
		spk::Engine &_engine;
		const spk::Texture *_atlas;
		std::unordered_map<Coordinate, std::unique_ptr<View>, CoordinateHash> _views;
		Collection::ChunkProvider::Contract _available;
		Collection::CoordinateProvider::Contract _removed;

	public:
		ViewCollection(spk::Engine &engine, const spk::Texture *atlas, Collection &chunks);

		[[nodiscard]] std::size_t size() const noexcept
		{
			return _views.size();
		}

		void setMesh(Coordinate coordinate, spk::TextureMesh3D mesh);
	};
}
