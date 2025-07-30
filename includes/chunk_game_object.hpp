#pragma once

#include <sparkle.hpp>

#include "chunk.hpp"

class ChunkGameObject final : public spk::GameObject
{
private:
	class Visualizer final : public spk::Component
	{
	private:
		BakableChunk::Renderer _renderer;

	public:
		explicit Visualizer(const std::wstring &name = L"Visualizer");

		void bake(const spk::SafePointer<BakableChunk> &chunk);
		void onPaintEvent(spk::PaintEvent &event) override;
	};

	spk::SafePointer<BakableChunk> _chunk;
	spk::SafePointer<Visualizer> _visualizer;
	spk::ContractProvider::Contract _onChunkEditionContract;

public:
	ChunkGameObject(const std::wstring &name, const spk::SafePointer<spk::GameObject> &parent = nullptr);

	void setChunk(const spk::SafePointer<BakableChunk> &chunk);
	spk::SafePointer<BakableChunk> chunk() const;
};