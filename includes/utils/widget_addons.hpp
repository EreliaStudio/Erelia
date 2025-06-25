#pragma once

#include <sparkle.hpp>

#include "atlas/asset_atlas.hpp"

class WidgetAddons
{
private:
	static inline AssetAtlas::Instanciator _assetAtlasInstanciator;

public:
	static inline spk::Font::Size defaultFontSize = {16, 5};
	static inline spk::Color defaultFontColor = spk::Color::white;
	static inline spk::Color defaultOutlineColor = spk::Color::black;
	
	template<typename T>
	static void centerInParent(T* p_object, const spk::Vector2UInt& p_childSize, const spk::Geometry2D& p_parentGeometry)
	{
		spk::Vector2Int widgetAnchor = (p_parentGeometry.size - p_childSize) / 2;
		p_object->setGeometry({widgetAnchor, p_childSize});
	}

	static void ApplyFormat(spk::SafePointer<spk::Frame> p_widget);
    static void ApplyFormat(spk::SafePointer<spk::PushButton> p_widget);
    static void ApplyFormat(spk::SafePointer<spk::ImageLabel> p_widget);
    static void ApplyFormat(spk::SafePointer<spk::TextLabel> p_widget);
    static void ApplyFormat(spk::SafePointer<spk::TextEdit> p_widget);
    static void ApplyFormat(spk::SafePointer<spk::TextArea> p_widget);
	static void ApplyFormat(spk::SafePointer<spk::IInterfaceWindow::MenuBar> p_menuBar);
	static void ApplyFormat(spk::SafePointer<spk::IInterfaceWindow> p_interfaceWindow);
};