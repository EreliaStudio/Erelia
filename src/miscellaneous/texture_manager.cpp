#include "texture_manager.hpp"

CustomTextureManager::CustomTextureManager()
{
	loadSpriteSheet("Frame1", "resources/texture/UI/Frame1.png", spk::Vector2UInt(3, 3));
	loadSpriteSheet("Frame2", "resources/texture/UI/Frame2.png", spk::Vector2UInt(3, 3));
	loadSpriteSheet("GameNameFrame", "resources/texture/UI/game_name_frame.png", spk::Vector2UInt(3, 3));
	loadSpriteSheet("DialogFrame", "resources/texture/UI/dialog_frame.png", spk::Vector2UInt(3, 3));
	
	loadSpriteSheet("MainMenuBackground", "resources/texture/main_menu/main_menu_background.png", spk::Vector2UInt(1, 1));
	loadSpriteSheet("MainMenuRiverAnimation", "resources/texture/main_menu/main_menu_river_animation.png", spk::Vector2UInt(1, 1));
	loadSpriteSheet("MainMenuSkyAnimation", "resources/texture/main_menu/main_menu_sky_animation.png", spk::Vector2UInt(1, 1));
	loadSpriteSheet("MainMenuTreeAnimation", "resources/texture/main_menu/main_menu_tree_animation.png", spk::Vector2UInt(1, 1));

	loadFont("Arial", "resources/font/Arial.ttf");
	loadFont("AVELIRE", "resources/font/AVELIRE.ttf");
	loadFont("VIKING-N", "resources/font/VIKING-N.ttf");
}

CustomTextureManager::~CustomTextureManager()
{
	for (auto& [key, element] : _fonts)
	{
		delete element;
	}
}

spk::Font* CustomTextureManager::loadFont(const std::string& p_fontName, const std::filesystem::path& p_fontPath)
{
	if (_fonts.contains(p_fontName) == true)
		spk::throwException("Can't load a font named [" + p_fontName + "] inside TextureManager : font already loaded");
	_fonts[p_fontName] = new spk::Font(p_fontPath);
	return (_fonts[p_fontName]);
}

spk::Font* CustomTextureManager::font(const std::string& p_fontName)
{
	if (_fonts.contains(p_fontName) == false)
		spk::throwException("Can't return a font named [" + p_fontName + "] inside TextureManager\nNo font loaded with desired name");
	return (_fonts[p_fontName]);
}