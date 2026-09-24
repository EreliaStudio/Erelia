#pragma once

#include "engine/registry.hpp"
#include "engine/component3d.hpp"
#include "geometry/texture_mesh_3d.hpp"
#include "math/matrix.hpp"

namespace spk
{
	class Texture;

	class TextureMeshRenderer3D : public spk::Component3D
	{
	private:
		const Texture *_texture = nullptr;
		TextureMesh3D _mesh;

		void _buildRenderSnapshot(RenderSnapshot::Builder &builder) override;

	public:
		TextureMeshRenderer3D(const std::string &name = "Unnamed texture mesh renderer 3D", Entity3D *owner = nullptr);
		explicit TextureMeshRenderer3D(Entity3D *owner);

		[[nodiscard]] const TextureMesh3D &mesh() const noexcept
		{
			return _mesh;
		}

		void setTexture(const Texture *texture) noexcept
		{
			_texture = texture;
		}
		void setMesh(TextureMesh3D mesh)
		{
			_mesh = std::move(mesh);
		}
	};
}
