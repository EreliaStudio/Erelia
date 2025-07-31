#include "chunk_game_object.hpp"

ChunkGameObject::Visualizer::Visualizer(const std::wstring &name) :
	spk::Component(name)
{
}

void ChunkGameObject::Visualizer::bake(const spk::SafePointer<BakableChunk> &p_chunk)
{
	_renderer.clear();

	if (p_chunk != nullptr)
	{
		_renderer.prepare(owner()->transform(), p_chunk);
	}

	_renderer.validate();
}

void ChunkGameObject::Visualizer::onPaintEvent(spk::PaintEvent &)
{
	_renderer.render();
}
