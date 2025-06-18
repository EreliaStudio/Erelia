#include "atlas/asset_atlas.hpp"

AssetAtlas::AssetAtlas()
{
	spk::JSON::File atlasFileConfiguration = spk::JSON::File("resources/assets/assets.json");
	
	load(atlasFileConfiguration);
}

void AssetAtlas::load(const spk::JSON::File& p_atlasFileConfiguration)
{
	if (p_atlasFileConfiguration.contains(L"Images") == true)
	{
		const auto& imagesNode = p_atlasFileConfiguration[L"Images"];

		if (imagesNode.isArray() == false)
		{
			throw std::runtime_error("The \"Images\" entry must be a JSON array");
		}

		for (const spk::JSON::Object* imagePtr : imagesNode.asArray())
		{
			const auto& image = *imagePtr;

			const std::wstring& name = image[L"Name"].as<std::wstring>();
			const std::wstring& path = image[L"Path"].as<std::wstring>();

			_images[name] = std::make_unique<spk::Image>(path);
		}
	}

	if (p_atlasFileConfiguration.contains(L"SpriteSheets") == true)
	{
		const auto& sheetsNode = p_atlasFileConfiguration[L"SpriteSheets"];

		if (sheetsNode.isArray() == false)
		{
			throw std::runtime_error("The \"SpriteSheets\" entry must be a JSON array");
		}

		for (const spk::JSON::Object* sheetPtr : sheetsNode.asArray())
		{
			const auto& sheet = *sheetPtr;

			const std::wstring& name = sheet[L"Name"].as<std::wstring>();
			const std::wstring& path = sheet[L"Path"].as<std::wstring>();

			spk::Vector2Int size(
				sheet[L"Size"][L"x"].as<long>(),
				sheet[L"Size"][L"y"].as<long>());

			_spriteSheets[name] = std::make_unique<spk::SpriteSheet>(path, size);
		}
	}

	if (p_atlasFileConfiguration.contains(L"Fonts") == true)
	{
		const auto& fontsNode = p_atlasFileConfiguration[L"Fonts"];

		if (fontsNode.isArray() == false)
		{
			throw std::runtime_error("The \"Fonts\" entry must be a JSON array");
		}

		for (const spk::JSON::Object* fontPtr : fontsNode.asArray())
		{
			const auto& font = *fontPtr;

			const std::wstring& name = font[L"Name"].as<std::wstring>();
			const std::wstring& path = font[L"Path"].as<std::wstring>();

			_fonts[name] = std::make_unique<spk::Font>(path);
		}
	}
}

spk::SafePointer<spk::Image> AssetAtlas::image(const std::wstring& p_name)
{
	if (_images.contains(p_name) == false)
	{
		throw std::runtime_error("Image not found: " + spk::StringUtils::wstringToString(p_name));
	}
	return (_images[p_name].get());
}

spk::SafePointer<spk::SpriteSheet> AssetAtlas::spriteSheet(const std::wstring& p_name)
{
	if (_spriteSheets.contains(p_name) == false)
	{
		throw std::runtime_error("SpriteSheet not found: " + spk::StringUtils::wstringToString(p_name));
	}
	return (_spriteSheets[p_name].get());
}

spk::SafePointer<spk::Font> AssetAtlas::font(const std::wstring& p_name)
{
	if (_fonts.contains(p_name) == false)
	{
		throw std::runtime_error("Font not found: " + spk::StringUtils::wstringToString(p_name));
	}
	return (_fonts[p_name].get());
}