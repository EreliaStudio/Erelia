#pragma once

#include <sparkle.hpp>

class AssetAtlas : public spk::Singleton<AssetAtlas>
{
	friend class spk::Singleton<AssetAtlas>;
private:
	/*
	{
		"Images":[
			{
				"Name":"Image",
				"Path":"assets/images/image.png"
			}
		],
		"SpriteSheets":[
			{
				"Name":"SpriteSheet",
				"Path":"assets/spritesheets/spritesheet.png",
				"Size":{
					"x":64,
					"y":64
				}
			}
		],
		"Fonts":[
			{
				"Name":"Font",
				"Path":"assets/fonts/font.ttf"
			}
		]
	}
	 */
	std::unordered_map<std::wstring, std::unique_ptr<spk::Image>> _images;
	std::unordered_map<std::wstring, std::unique_ptr<spk::SpriteSheet>> _spriteSheets;
	std::unordered_map<std::wstring, std::unique_ptr<spk::Font>> _fonts;

	AssetAtlas();

public:
	void load(const spk::JSON::File& p_atlasFileConfiguration);
	
	spk::SafePointer<spk::Image> image(const std::wstring& p_name);
	spk::SafePointer<spk::SpriteSheet> spriteSheet(const std::wstring& p_name);
	spk::SafePointer<spk::Font> font(const std::wstring& p_name);
};