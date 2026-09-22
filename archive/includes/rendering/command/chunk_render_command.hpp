#pragma once

#include <sparkle>
#include "rendering/command/texture_mesh_3d_render_command.hpp"

namespace playground
{
	class ChunkRenderCommand final : public spk::RenderCommand
	{
		spk::TextureMesh3DRenderCommand _renderer;
		spk::Profiler::TimeMeasurement &_renderTime;
		spk::Profiler::GaugeMeasurement<std::size_t> &_vertices;
		spk::Profiler::GaugeMeasurement<std::size_t> &_indexes;
		std::size_t _vertexCount;
		std::size_t _indexCount;

	public:
		ChunkRenderCommand(const spk::Texture *texture, spk::TextureMesh3D mesh, spk::Matrix4x4 model, spk::Profiler &profiler) :
			_renderer(texture, mesh, model),
			_renderTime(profiler.timeMeasurement("Chunks/render")),
			_vertices(profiler.gaugeMeasurement<std::size_t>("Chunks/vertices per chunk")),
			_indexes(profiler.gaugeMeasurement<std::size_t>("Chunks/indexes per chunk")),
			_vertexCount(mesh.vertexCount()),
			_indexCount(mesh.indexCount())
		{
		}

		void execute(spk::RenderContext &context) const override
		{
			spk::Profiler::TimeMeasurement::Scope scope(_renderTime);
			_renderer.execute(context);
			_vertices.insert(_vertexCount);
			_indexes.insert(_indexCount);
		}
	};
}
