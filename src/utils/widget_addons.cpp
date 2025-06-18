#include "utils/widget_addons.hpp"

void WidgetAddons::ApplyFormat(spk::SafePointer<spk::Frame> p_widget)
{
	p_widget->setNineSlice(AssetAtlas::instance()->spriteSheet(L"defaultNineSlice"));
}

void WidgetAddons::ApplyFormat(spk::SafePointer<spk::PushButton> p_widget)
{
	p_widget->setNineSlice(AssetAtlas::instance()->spriteSheet(L"defaultNineSlice"), spk::PushButton::State::Released);
	p_widget->setNineSlice(AssetAtlas::instance()->spriteSheet(L"defaultNineSlice_Dark"), spk::PushButton::State::Pressed);
	p_widget->setNineSlice(AssetAtlas::instance()->spriteSheet(L"defaultNineSlice_Light"), spk::PushButton::State::Hovered);

	p_widget->setFont(AssetAtlas::instance()->font(L"defaultFont"));
	p_widget->setFontColor(defaultFontColor, defaultOutlineColor);
	p_widget->setFontSize(defaultFontSize);
}

void WidgetAddons::ApplyFormat(spk::SafePointer<spk::TextLabel> p_widget)
{
	p_widget->setNineSlice(AssetAtlas::instance()->spriteSheet(L"defaultNineSlice"));
	p_widget->setTextAlignment(spk::HorizontalAlignment::Centered, spk::VerticalAlignment::Centered);
	p_widget->setFont(AssetAtlas::instance()->font(L"defaultFont"));
	p_widget->setFontColor(defaultFontColor, defaultOutlineColor);
	p_widget->setFontSize(defaultFontSize);
}

void WidgetAddons::ApplyFormat(spk::SafePointer<spk::TextEdit> p_widget)
{
	p_widget->setNineSlice(AssetAtlas::instance()->spriteSheet(L"defaultNineSlice"));
	p_widget->setFont(AssetAtlas::instance()->font(L"defaultFont"));
	p_widget->setFontColor(defaultFontColor, defaultOutlineColor);
	p_widget->setFontSize(defaultFontSize);
}

void WidgetAddons::ApplyFormat(spk::SafePointer<spk::TextArea> p_widget)
{
	p_widget->setNineSlice(AssetAtlas::instance()->spriteSheet(L"defaultNineSlice"));
	p_widget->setFont(AssetAtlas::instance()->font(L"defaultFont"));
	p_widget->setFontColor(defaultFontColor, defaultOutlineColor);
	p_widget->setFontSize(defaultFontSize);
}

void WidgetAddons::ApplyFormat(spk::SafePointer<spk::IInterfaceWindow::MenuBar> p_menuBar)
{
	ApplyFormat(p_menuBar->titleLabel());

	ApplyFormat(p_menuBar->minimizeButton());
	ApplyFormat(p_menuBar->maximizeButton());
	ApplyFormat(p_menuBar->closeButton());
}

void WidgetAddons::ApplyFormat(spk::SafePointer<spk::IInterfaceWindow> p_interfaceWindow)
{
	ApplyFormat(p_interfaceWindow->menuBar());

	ApplyFormat(p_interfaceWindow->backgroundFrame());
	ApplyFormat(p_interfaceWindow->minimizedBackgroundFrame());
}