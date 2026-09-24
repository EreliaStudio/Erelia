#include "engine/texture_mesh_renderer_3d.hpp"

#include "engine/engine.hpp"
#include "engine/entity3d.hpp"
#include "engine/transform3d.hpp"
#include "rendering/command/texture_mesh_3d_render_command.hpp"

namespace spk
{
	TextureMeshRenderer3D::TextureMeshRenderer3D(const std::string &name, Entity3D *owner) :
		Component3D(name, owner)
	{
	}
	TextureMeshRenderer3D::TextureMeshRenderer3D(Entity3D *owner) :
		TextureMeshRenderer3D("Unnamed texture mesh renderer 3D", owner)
	{
	}
	void TextureMeshRenderer3D::_buildRenderSnapshot(RenderSnapshot::Builder &builder)
	{
		if (_texture && !_mesh.empty() && owner())
		{
			builder.renderPass(Engine::SceneRenderPassKey).emplace<TextureMesh3DRenderCommand>(_texture, _mesh, owner()->transform().modelMatrix());
		}
	}
}
