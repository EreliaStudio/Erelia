#include "engine/chunk_render_system.hpp"

#include "engine/entity3d.hpp"
#include "rendering/command/chunk_render_command.hpp"
#include "voxel/chunk_renderer.hpp"

namespace playground
{
	void ChunkRenderSystem::_buildRenderSnapshot(spk::RenderSnapshot::Builder &builder)
	{
		if (engine() == nullptr) return;
		spk::Profiler::TimeMeasurement::Scope group(_profiler->timeMeasurement("Chunks/build group"));
		std::size_t chunks = 0, vertices = 0, indexes = 0;
		auto &pass = builder.renderPass(spk::Engine::SceneRenderPassKey);
		for (spk::Component3D *base : spk::Registry<spk::Component3D, spk::Engine *>::instance().elements(engine()))
		{
			auto *renderer = dynamic_cast<voxel::ChunkRenderer *>(base);
			if (renderer == nullptr || renderer->texture() == nullptr || renderer->mesh().empty() || renderer->owner() == nullptr) continue;
			++chunks;
			vertices += renderer->mesh().vertexCount();
			indexes += renderer->mesh().indexCount();
			{
				spk::Profiler::TimeMeasurement::Scope buildChunk(_profiler->timeMeasurement("Chunks/build chunk"));
				pass.emplace<playground::ChunkRenderCommand>(renderer->texture(), renderer->mesh(), renderer->owner()->transform().modelMatrix(), *_profiler);
			}
		}
		_profiler->gaugeMeasurement<std::size_t>("Chunks/count").insert(chunks);
		_profiler->gaugeMeasurement<std::size_t>("Chunks/vertices").insert(vertices);
		_profiler->gaugeMeasurement<std::size_t>("Chunks/indexes").insert(indexes);
	}
}
