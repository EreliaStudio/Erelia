#include "voxel/chunk_view.hpp"

#include <utility>

#include "voxel/chunk.hpp"

namespace voxel
{
	Chunk::View::View(spk::Engine &engine, const spk::Texture *atlas, Coordinate coordinate) :
		_engine(engine),
		_entity("Voxel chunk"),
		_renderer(&_entity.addComponent<ChunkRenderer>("Chunk renderer"))
	{
		_renderer->setTexture(atlas);
		const auto origin = Chunk::worldOrigin(coordinate);
		_entity.transform().place(spk::Vector3(origin));
		_engine.addEntity(&_entity);
	}

	Chunk::View::~View()
	{
		_engine.removeEntity(&_entity);
	}

	void Chunk::View::setMesh(spk::TextureMesh3D mesh)
	{
		_renderer->setMesh(std::move(mesh));
	}
}
