#pragma once

#include "sparkle.hpp"

class CustomTextureManager : public spk::TextureManager
{
private:
	std::map<std::string, spk::Font*> _fonts;

public:
	CustomTextureManager();
	~CustomTextureManager();

	spk::Font* loadFont(const std::string& p_fontName, const std::filesystem::path& p_fontPath);
	spk::Font* font(const std::string& p_fontName);
};

using TextureManager = spk::Singleton<CustomTextureManager>;