#pragma once

#include <sparkle.hpp>

#include "node.hpp"

using Chunk = spk::IChunk<Node::ID, 16, 16, 5>;

class BakableChunk : public Chunk
{
public:
	class Renderer
	{
	private:
		static inline std::unique_ptr<spk::Lumina::Shader> _shader;
		spk::Lumina::Shader::Object _object;
		spk::OpenGL::BufferSet& _bufferSet;
		spk::OpenGL::UniformBufferObject &_transformUBO;

		static inline spk::SafePointer<spk::SpriteSheet> _tileset;

		static spk::Lumina::Shader::Object createObject();

		void _insertData(spk::Vector3 p_anchor, spk::Vector2 p_size, const spk::SpriteSheet::Sprite& p_sprite, const Node::ID& p_nodeID);

		struct PrepareData
		{
			spk::SafePointer<BakableChunk> chunk;
			spk::SafePointer<BakableChunk> neightbours[3][3];

			void setup(const spk::SafePointer<BakableChunk>& p_chunk);
			spk::SafePointer<Chunk> neighbourChunk(const spk::Vector2Int& p_offset) const;
			spk::Vector2Int neighbourOffset(const spk::Vector3Int& p_position) const;
		};

		void _prepareAutotile(size_t i, size_t j, size_t k, const Node::ID& nodeID, const Node* node, const PrepareData &p_prepareData);
		void _prepareMonotile(size_t i, size_t j, size_t k, const Node::ID& nodeID, const Node* node, const PrepareData &p_prepareData);

		spk::Vector2Int _computeCornerOffset(size_t i, size_t j, size_t k, size_t corner, const Node::ID& nodeID, const PrepareData &p_prepareData);

	public:
		Renderer();

		static void setTileset(const spk::SafePointer<spk::SpriteSheet> &p_tileset);

		void clear();
		void prepare(const spk::Transform& p_transform, const spk::SafePointer<BakableChunk> &p_chunk);
		void validate();
		void render();
	};

private:
	spk::Vector2Int _coordinates{};
	spk::ContractProvider _onEditionContractProvider;

public:
	BakableChunk() = default;
	explicit BakableChunk(const spk::Vector2Int &coordinates);

	spk::ContractProvider::Contract subscribeToEdition(const spk::ContractProvider::Job &p_job);

	void setCoordinates(const spk::Vector2Int &coordinates);
	const spk::Vector2Int& coordinates() const;
};