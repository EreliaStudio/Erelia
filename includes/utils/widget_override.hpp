#pragma once

#include <sparkle.hpp>

#include "utils/widget_addons.hpp"

class PushButton : public spk::PushButton
{
private:
	spk::PushButton::Contract _onClickContract;

public:
	PushButton(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	void setOnClick(const std::function<void()>& p_callback);
	void configure(const std::wstring& p_text, const spk::PushButton::Job& p_job);
};

class TextLabel : public spk::TextLabel
{
private:

public:
	TextLabel(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);
};

class TextEdit : public spk::TextEdit
{
private:

public:
	TextEdit(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);
};

class TextArea : public spk::TextArea
{
private:

public:
	TextArea(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);
};

class Frame : public spk::Frame
{
private:

public:
	Frame(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);
};

class CommandPanel : public spk::CommandPanel
{
public:
	struct Descriptor
	{
		std::wstring text;
		spk::PushButton::Job job;
	};

private:
	struct ButtonData
    {
        spk::SafePointer<spk::PushButton> button;
        spk::PushButton::Contract contract;
    };

    std::unordered_map<std::wstring, ButtonData> _buttonContainer;

	using spk::CommandPanel::addButton;

public:
	CommandPanel(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);
	CommandPanel(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent, std::initializer_list<Descriptor> descriptors);

    spk::SafePointer<spk::PushButton> addButton(const std::wstring& p_name, const std::wstring& p_text, const spk::PushButton::Job& p_job);
	void setOnClick(const std::wstring& p_name, const spk::PushButton::Job& p_callback);
};

template<typename WidgetType>
struct FormRow : public spk::FormLayout::Row<WidgetType>
{
	FormRow(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
		spk::FormLayout::Row<WidgetType>(p_name, p_parent)
	{
		WidgetAddons::ApplyFormat(&(this->label));
	}
};

class MessageBox : public spk::MessageBox
{
private:
	struct ButtonData
    {
        spk::SafePointer<spk::PushButton> button;
        spk::PushButton::Contract         contract;
    };

    std::map<std::wstring, ButtonData> _instanciedButtons;

	using spk::MessageBox::addButton;

public:
	MessageBox(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	void setText(const std::wstring& p_text) override;

	spk::SafePointer<spk::PushButton> addButton(const std::wstring& p_name, const std::wstring& p_label, const spk::PushButton::Job& p_job);

	void setButtonCallback(const std::wstring& p_buttonName, const spk::PushButton::Job& p_job);

	void show(const std::wstring& p_text);
};

class InformationMessageBox : public spk::InformationMessageBox
{
public:
    InformationMessageBox(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	void setText(const std::wstring& p_text) override;
	void show(const std::wstring& p_text);
};

class RequestMessageBox : public spk::RequestMessageBox
{
private:

public:
    RequestMessageBox(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	void setText(const std::wstring& p_text) override;
	void show(const std::wstring& p_text);
};