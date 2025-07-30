#include "chunk_game_object.hpp"

ChunkGameObject::Visualizer::Visualizer(const std::wstring &name) :
	spk::Component(name)
{
}

void ChunkGameObject::Visualizer::bake(const spk::SafePointer<BakableChunk> &chunk)
{
	if (chunk == nullptr)
	{
		return;
	}
	_renderer.clear();
	_renderer.prepare(chunk);
	_renderer.validate();
}

void ChunkGameObject::Visualizer::onPaintEvent(spk::PaintEvent &)
{
	_renderer.render();
}
