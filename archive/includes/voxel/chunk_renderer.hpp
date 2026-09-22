#pragma once

#include <sparkle>

namespace voxel
{
	class ChunkRenderer final : public spk::Component3D
	{
		const spk::Texture *_texture = nullptr;
		spk::TextureMesh3D _mesh;

	public:
		ChunkRenderer(const std::string &name = "Chunk renderer", spk::Entity3D *owner = nullptr) : spk::Component3D(name, owner) {}
		ChunkRenderer(spk::Entity3D *owner) : ChunkRenderer("Chunk renderer", owner) {}
		void setTexture(const spk::Texture *texture) noexcept { _texture = texture; }
		void setMesh(spk::TextureMesh3D mesh) { _mesh = std::move(mesh); }
		[[nodiscard]] const spk::Texture *texture() const noexcept { return _texture; }
		[[nodiscard]] const spk::TextureMesh3D &mesh() const noexcept { return _mesh; }
	};
}
