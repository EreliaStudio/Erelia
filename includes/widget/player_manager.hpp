#pragma once

#include <sparkle.hpp>

#include "widget/graphical_widget.hpp"

class PlayerManager : public GraphicalWidget
{
private:
	enum class ActionType
	{
		MotionUp,
		MotionDown,
		MotionLeft,
		MotionRight	
	};
	UniformBufferObjectAtlas::Instanciator _uniformBufferObjectAtlasInstanciator;

	spk::Vector2Int _screenRenderDimension;
	spk::OpenGL::UniformBufferObject& _cameraUBO;
	Actor::Contract _onPlayerEditionContract;

	spk::Camera _camera;

	std::unordered_map<ActionType, std::unique_ptr<spk::Action>> _actions;

	void _updatePlayerUBOs();

	void _onGeometryChange() override;
	void _onPaintEvent(spk::PaintEvent& p_event) override;

public:
	PlayerManager(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	void initialize();
};