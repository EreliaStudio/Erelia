#include "ssbo_factory.hpp"

spk::OpenGL::ShaderStorageBufferObject& SSBOFactory::nodeCollectionSSBO()
{
	if (spk::Lumina::Shader::Constants::containsSSBO(L"nodeCollectionSSBO") == false)
	{
		/*
		------ C++ ------
		struct Node
		{
			using ID = int;

			struct Animation
			{
				int nbFrame;
				spk::Vector2Int offsetPerFrame;
				int duration;
			};

			spk::Vector2Int sprite;
			Animation animation;
		};

		------ glsl ------
		struct Node
		{
			ivec2 sprite;
			struct Animation
			{
				int nbFrame;
				ivec2 offsetPerFrame;
				int duration;
			} animation;
		};

		layout(std430, binding = 2) buffer NodeCollectionSSBO
		{
			int nbNode;
			Node nodes[];
		} nodeCollectionSSBO;
		*/
		spk::OpenGL::ShaderStorageBufferObject newSSBO = spk::OpenGL::ShaderStorageBufferObject(L"NodeCollectionSSBO", 2, 4, 8, 32, 0);

		newSSBO.fixedData().addElement(L"nbNode", 0, 4);

		auto& spriteElement = newSSBO.dynamicArray().addElement(L"sprite", 0, 8);
		auto& animationElement = newSSBO.dynamicArray().addElement(L"animation", 8, 24);

		animationElement.addElement(L"nbFrame", 0, 4);
		animationElement.addElement(L"offsetPerFrame", 8, 8);
		animationElement.addElement(L"duration", 16, 4);

		spk::Lumina::Shader::Constants::addSSBO(L"nodeCollectionSSBO", std::move(newSSBO));
	}
	return (spk::Lumina::Shader::Constants::ssbo(L"nodeCollectionSSBO"));
}