#pragma once

#include "sparkle.hpp"

#include "miscellaneous/texture_manager.hpp"

class Frame : public spk::Frame
{
private:

public:
	Frame(Widget* p_parent) :
		Frame("Anonymous Frame", p_parent)
	{

	}
	Frame(const std::string& p_name, Widget* p_parent) :
		spk::Frame(p_name, p_parent)
	{
		box().setSpriteSheet(TextureManager::instance()->spriteSheet("Frame1"));
		box().setCornerSize(8);
	}
};

class TextLabel : public spk::TextLabel
{
private:

public:
	TextLabel(Widget* p_parent) :
		TextLabel("Anonymous TextLabel", p_parent)
	{

	}
	TextLabel(const std::string& p_name, Widget* p_parent) :
		spk::TextLabel(p_name, p_parent)
	{
		box().setSpriteSheet(TextureManager::instance()->spriteSheet("Frame1"));
		box().setCornerSize(8);

		label().setFont(TextureManager::instance()->font("Arial"));
		label().setVerticalAlignment(spk::VerticalAlignment::Top);
		label().setHorizontalAlignment(spk::HorizontalAlignment::Left);

		label().setTextSize(25);
		label().setTextColor(spk::Color(220, 220, 220));

		label().setOutlineSize(3);
		label().setOutlineColor(spk::Color(0, 0, 0));
		label().setOutlineStyle(spk::Font::OutlineStyle::Standard);
	}
};

class TextEntry : public spk::TextEntry
{
private:

public:
	TextEntry(Widget* p_parent) :
		TextEntry("Anonymous TextEntry", p_parent)
	{

	}
	TextEntry(const std::string& p_name, Widget* p_parent) :
		spk::TextEntry(p_name, p_parent)
	{
		box().setSpriteSheet(TextureManager::instance()->spriteSheet("Frame1"));
		box().setCornerSize(8);

		setPlaceholder("Enter text ...");

		label().setFont(TextureManager::instance()->font("Arial"));
		label().setVerticalAlignment(spk::VerticalAlignment::Top);
		label().setHorizontalAlignment(spk::HorizontalAlignment::Left);

		label().setTextSize(25);
		label().setTextColor(spk::Color(220, 220, 220));

		label().setOutlineSize(3);
		label().setOutlineColor(spk::Color(0, 0, 0));
		label().setOutlineStyle(spk::Font::OutlineStyle::Standard);
	}
};

class Button : public spk::Button
{
private:

public:
	Button(Widget* p_parent) :
		Button("Anonymous Button", p_parent)
	{

	}
	Button(const std::string& p_name, Widget* p_parent) :
		spk::Button(p_name, p_parent)
	{
		box(spk::Button::State::Released).setSpriteSheet(TextureManager::instance()->spriteSheet("Frame1"));
		box(spk::Button::State::Released).setCornerSize(8); 
		
		box(spk::Button::State::Pressed).setSpriteSheet(TextureManager::instance()->spriteSheet("Frame2"));
		box(spk::Button::State::Pressed).setCornerSize(8);

		label(spk::Button::State::Released).setFont(TextureManager::instance()->font("Arial"));
		label(spk::Button::State::Released).setVerticalAlignment(spk::VerticalAlignment::Centered);
		label(spk::Button::State::Released).setHorizontalAlignment(spk::HorizontalAlignment::Centered);

		label(spk::Button::State::Released).setTextSize(25);
		label(spk::Button::State::Released).setTextColor(spk::Color(220, 220, 220));

		label(spk::Button::State::Released).setOutlineSize(1);
		label(spk::Button::State::Released).setOutlineColor(spk::Color(0, 0, 0));
		label(spk::Button::State::Released).setOutlineStyle(spk::Font::OutlineStyle::Standard);
		
		label(spk::Button::State::Pressed).setFont(TextureManager::instance()->font("Arial"));
		label(spk::Button::State::Pressed).setVerticalAlignment(spk::VerticalAlignment::Centered);
		label(spk::Button::State::Pressed).setHorizontalAlignment(spk::HorizontalAlignment::Centered);

		label(spk::Button::State::Pressed).setTextSize(23);
		label(spk::Button::State::Pressed).setTextColor(spk::Color(200, 200, 200));

		label(spk::Button::State::Pressed).setOutlineSize(1);
		label(spk::Button::State::Pressed).setOutlineColor(spk::Color(0, 0, 0));
		label(spk::Button::State::Pressed).setOutlineStyle(spk::Font::OutlineStyle::Standard);
	}
};