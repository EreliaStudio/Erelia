#pragma once

#include <sparkle.hpp>

#include "widget/graphical_widget.hpp"

class PlayerManager : public GraphicalWidget
{
public:
	enum class Mode
	{
		Normal,
		Creative
	};

private:	
	UniformBufferObjectAtlas::Instanciator _uniformBufferObjectAtlasInstanciator;

	spk::Vector2 _screenRenderDimension;
	spk::OpenGL::UniformBufferObject& _cameraUBO;
	Actor::Contract _onPlayerEditionContract;

	spk::Camera _camera;

	std::vector<std::unique_ptr<spk::Action>> _actions;
	Mode _mode = Mode::Creative;
	spk::SafePointer<const spk::Mouse> _mouse;
	spk::SafePointer<const spk::Keyboard> _keyboard;

	struct CreativeData
	{
		NodeMap::ID nodeToPlace = 0;
	};

	CreativeData _creativeData;

	void _updatePlayerUBOs();

	void _onGeometryChange() override;
	void _onUpdateEvent(spk::UpdateEvent& p_event) override;

public:
	PlayerManager(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent);

	void setMode(const Mode& p_mode);
	Mode mode() const;

	void initialize();
};