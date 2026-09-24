#include "voxel/chunk_view_collection.hpp"

#include <utility>

namespace voxel
{
	Chunk::ViewCollection::ViewCollection(spk::Engine &engine, const spk::Texture *atlas, Collection &chunks) :
		_engine(engine),
		_atlas(atlas),
		_available(chunks.subscribeToAvailability([this](const Chunk &chunk) {
			const auto coordinate = chunk.coordinate();
			_views.emplace(coordinate, std::make_unique<View>(_engine, _atlas, coordinate));
		})),
		_removed(chunks.subscribeToRemoval([this](Coordinate coordinate) {
			_views.erase(coordinate);
		}))
	{
	}

	void Chunk::ViewCollection::setMesh(Coordinate coordinate, spk::TextureMesh3D mesh)
	{
		auto iterator = _views.find(coordinate);
		if (iterator != _views.end())
		{
			iterator->second->setMesh(std::move(mesh));
		}
	}
}
