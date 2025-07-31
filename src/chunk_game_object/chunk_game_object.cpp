#include "chunk_game_object.hpp"

ChunkGameObject::ChunkGameObject(const std::wstring &name, const spk::SafePointer<spk::GameObject> &parent) :
	spk::GameObject(name, parent),
	_visualizer(addComponent<Visualizer>(name + L"/Visualizer"))
{
	transform().place({0, 0, 0});
}

void ChunkGameObject::setChunk(const spk::SafePointer<BakableChunk> &chunk)
{
	_chunk = chunk;
	_visualizer->bake(_chunk);

	if (_chunk == nullptr)
	{
		return;
	}

	transform().place( {
		static_cast<float>(_chunk->coordinates().x * BakableChunk::Size.x),
		static_cast<float>(_chunk->coordinates().y * BakableChunk::Size.y),
		0.f
	});

	_onChunkEditionContract = _chunk->subscribeToEdition([this]() { _visualizer->bake(_chunk); });
}

spk::SafePointer<BakableChunk> ChunkGameObject::chunk() const
{
	return _chunk;
}
