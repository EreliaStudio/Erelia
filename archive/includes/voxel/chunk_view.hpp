#pragma once

#include <sparkle>

#include "voxel/chunk_renderer.hpp"
#include "voxel/chunk.hpp"

namespace voxel
{
	class Chunk::View final
	{
	private:
		spk::Engine &_engine;
		spk::Entity3D _entity;
		ChunkRenderer *_renderer;

	public:
		View(spk::Engine &engine, const spk::Texture *atlas, Coordinate coordinate);

		~View();

		void setMesh(spk::TextureMesh3D mesh);
	};
}
