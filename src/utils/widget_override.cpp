#include "utils/widget_override.hpp"
#include "utils/widget_addons.hpp"

PushButton::PushButton(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::PushButton(p_name, p_parent)
{
	WidgetAddons::ApplyFormat(this);
}

void PushButton::setOnClick(const std::function<void()>& p_callback)
{
	_onClickContract = subscribe(p_callback);
}

void PushButton::configure(const std::wstring& p_text, const spk::PushButton::Job& p_job)
{
	setText(p_text);
	setOnClick(p_job);
}

TextLabel::TextLabel(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::TextLabel(p_name, p_parent)
{
	WidgetAddons::ApplyFormat(this);
}

TextEdit::TextEdit(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::TextEdit(p_name, p_parent)
{
	WidgetAddons::ApplyFormat(this);
}

TextArea::TextArea(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::TextArea(p_name, p_parent)
{
	WidgetAddons::ApplyFormat(this);
}

Frame::Frame(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::Frame(p_name, p_parent)
{
	WidgetAddons::ApplyFormat(this);
}

CommandPanel::CommandPanel(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::CommandPanel(p_name, p_parent)
{

}

CommandPanel::CommandPanel(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent, std::initializer_list<Descriptor> descriptors) :
	spk::CommandPanel(p_name, p_parent)
{
	for (const auto& descriptor : descriptors)   
	{
		addButton(p_name + L"/" + descriptor.text + L"Button", descriptor.text, descriptor.job);
	}
}

spk::SafePointer<spk::PushButton> CommandPanel::addButton(const std::wstring& p_name, const std::wstring& p_text, const spk::PushButton::Job& p_job)
{
	spk::SafePointer<spk::PushButton> newButton = spk::CommandPanel::addButton(p_name, p_text);

	WidgetAddons::ApplyFormat(newButton);

	_buttonContainer.emplace(
		p_name,
		ButtonData{
			newButton,
			newButton->subscribe(p_job)
		});

	return newButton;
}

void CommandPanel::setOnClick(const std::wstring& p_name, const spk::PushButton::Job& p_callback)
{
	if (_buttonContainer.contains(p_name) == false)
	{
	throw std::out_of_range("Button with name '" + spk::StringUtils::wstringToString(p_name) + "' not found in CommandPanel");
	}

	auto& buttonData = _buttonContainer[p_name];
	
	buttonData.contract = buttonData.button->subscribe(p_callback);
}

MessageBox::MessageBox(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) : 
	spk::MessageBox(p_name, p_parent)
{
	WidgetAddons::ApplyFormat(this);

	WidgetAddons::ApplyFormat(&(textArea()));

	setMenuHeight(25);
	setTitle(L"Unnamed message box");
}

void MessageBox::setText(const std::wstring& p_text)
{
	spk::MessageBox::setText(p_text);
	setMinimalWidth(commandPanel().minimalSize().x * 2);
	parent()->requireGeometryUpdate();
}

spk::SafePointer<spk::PushButton> MessageBox::addButton(const std::wstring& p_name, const std::wstring& p_label, const spk::PushButton::Job& p_job)
{
	auto newButton = commandPanel().addButton(p_name, p_label);

	WidgetAddons::ApplyFormat(newButton);

	_instanciedButtons.emplace(p_name, ButtonData{
		newButton,
		newButton->subscribe(p_job)
	});

	return newButton;
}

void MessageBox::setButtonCallback(const std::wstring& p_buttonName, const spk::PushButton::Job& p_job)
{
	if (_instanciedButtons.contains(p_buttonName) == false)
	{
		throw std::runtime_error("Button not found");
	}
	
	auto& buttonHolder = _instanciedButtons[p_buttonName];

	buttonHolder.contract = buttonHolder.button->subscribe(p_job);
}

void MessageBox::show(const std::wstring& p_text)
{
	setText(p_text);
	activate();
}

InformationMessageBox::InformationMessageBox(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::InformationMessageBox(p_name, p_parent)
{
	WidgetAddons::ApplyFormat(this);

	WidgetAddons::ApplyFormat(&(textArea()));

	WidgetAddons::ApplyFormat(button());
}

void InformationMessageBox::setText(const std::wstring& p_text)
{
	spk::MessageBox::setText(p_text);
	setMinimalWidth(commandPanel().minimalSize().x * 2);
	parent()->requireGeometryUpdate();
}

void InformationMessageBox::show(const std::wstring& p_text)
{
	setText(p_text);
	activate();
}

RequestMessageBox::RequestMessageBox(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::RequestMessageBox(p_name, p_parent)
{
	WidgetAddons::ApplyFormat(this);

	WidgetAddons::ApplyFormat(&(textArea()));

	WidgetAddons::ApplyFormat(firstButton());
	WidgetAddons::ApplyFormat(secondButton());
}

void RequestMessageBox::setText(const std::wstring& p_text)
{
	spk::MessageBox::setText(p_text);
	setMinimalWidth(commandPanel().minimalSize().x * 2);
	parent()->requireGeometryUpdate();
}

void RequestMessageBox::show(const std::wstring& p_text)
{
	setText(p_text);
	activate();
}