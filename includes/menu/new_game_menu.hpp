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

		spk::FormLayout _layout;

		spk::FormLayout::Row<spk::TextEdit> _nameRow;
		spk::FormLayout::Row<SeedField> _seedRow;
		spk::SpacerWidget _spacer;

		void _onGeometryChange();

	public:
		Content(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

		const std::wstring& name() const;
		const std::wstring& seed() const;
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

	void onConfirmRequest(const spk::PushButton::Job& p_job);
	void onCancelRequest(const spk::PushButton::Job& p_job);
};