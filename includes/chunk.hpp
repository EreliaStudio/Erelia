#pragma once

#include <sparkle.hpp>

#include "node.hpp"
#include "ssbo_factory.hpp"
#include "ubo_factory.hpp"

using Chunk = spk::IChunk<Node::ID, 16, 16, 5>;

class BakableChunk : public Chunk
{
public:
	class Renderer
	{
	private:
		static inline std::unique_ptr<spk::Lumina::Shader> _shader;
		spk::Lumina::Shader::Object _object;
		spk::OpenGL::UniformBufferObject &_cameraUBO;
		spk::OpenGL::ShaderStorageBufferObject &_nodeCollectionSSBO;

		static spk::Lumina::Shader::Object createObject();

	public:
		Renderer();

		void clear();
		void prepare(const spk::SafePointer<BakableChunk> &chunk);
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