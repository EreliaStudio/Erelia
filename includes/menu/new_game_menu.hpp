#pragma once

#include <sparkle.hpp>

#include "utils/widget_override.hpp"

class NewGameMenu : public spk::Screen
{
private:
	class Content : public spk::Widget
	{
	private:
		class SeedField : public spk::Widget
		{
		private:
			spk::HorizontalLayout _layout;
			TextEdit _field;
			PushButton _generateButton;
			spk::RandomGenerator<int> _seedGenerator;

			void _onGeometryChange();

		public:
			SeedField(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

			void generateSeed();

			spk::TextEdit& field();

			PushButton& generateButton();

			const std::wstring& seed() const;
		};

		class IconSelectorField : public spk::Widget
		{
		private:
			spk::SafePointer<spk::SpriteSheet> _spriteSheet;
			spk::Vector2UInt _iconSprite;

			spk::HorizontalLayout _layout;
			PushButton _negativeButton;
			spk::SpacerWidget _spacerA;
			spk::ImageLabel _imageLabel;
			spk::SpacerWidget _spacerB;
			PushButton _positiveButton;

			void _onGeometryChange();

		public:
			IconSelectorField(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

			void setSpriteSheet(spk::SafePointer<spk::SpriteSheet> p_spriteSheet);
			spk::SafePointer<spk::SpriteSheet> spriteSheet() const;

			void setIconSprite(const spk::Vector2UInt& p_iconSprite);
			const spk::Vector2UInt& iconSprite() const;
		};

		spk::FormLayout _layout;

		spk::FormLayout::Row<spk::TextEdit> _nameRow;
		spk::FormLayout::Row<SeedField> _seedRow;
		spk::FormLayout::Row<IconSelectorField> _iconSelectorRow;
		spk::SpacerWidget _spacer;

		void _onGeometryChange();

	public:
		Content(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

		const std::wstring& name() const;
		const std::wstring& seed() const;
		const spk::Vector2UInt iconSprite() const;
	};

	spk::VerticalLayout _layout;

	Content _content;
	CommandPanel _commandPanel;
	spk::SafePointer<spk::PushButton> _confirmButton;
	spk::SafePointer<spk::PushButton> _cancelButton;

	std::function<void()> _onConfirmLambda;

	RequestMessageBox _saveConfirmationRequestMessageBox;

	void _onGeometryChange();

public:
	NewGameMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	const std::wstring& name() const;
	const std::wstring& seed() const;
	const spk::Vector2UInt iconSprite() const;

	void onConfirmRequest(const spk::PushButton::Job& p_job);
	void onCancelRequest(const spk::PushButton::Job& p_job);
};