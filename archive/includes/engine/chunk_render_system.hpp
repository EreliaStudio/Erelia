#pragma once

#include <sparkle>

namespace playground
{
	class ChunkRenderSystem final : public spk::System
	{
		spk::Profiler *_profiler;

	protected:
		void _buildRenderSnapshot(spk::RenderSnapshot::Builder &builder) override;

	public:
		ChunkRenderSystem(spk::Profiler &profiler, spk::Engine *engine = nullptr) : spk::System("Chunk render system", engine), _profiler(&profiler) {}
	};
}
