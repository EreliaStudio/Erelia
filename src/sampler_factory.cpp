#include "sampler_factory.hpp"

spk::OpenGL::SamplerObject& SamplerFactory::tilesetTextureSampler()
{
	if (spk::Lumina::Shader::Constants::containsSampler(L"tilesetTexture") == false)
	{
		/*
		------ glsl ------
		layout(location = 4) uniform sampler2D tilesetTexture;
		*/

		spk::OpenGL::SamplerObject newSampler = spk::OpenGL::SamplerObject("tilesetTexture", spk::OpenGL::SamplerObject::Type::Texture2D, 4);

		spk::Lumina::Shader::Constants::addSampler(L"tilesetTexture", std::move(newSampler));
	}
	return (spk::Lumina::Shader::Constants::sampler(L"tilesetTexture"));
}
