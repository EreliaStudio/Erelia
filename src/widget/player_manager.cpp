#include "widget/player_manager.hpp"

#include "structure/context.hpp"

void PlayerManager::_updatePlayerUBOs()
{
	const spk::Vector2 playerPos = Context::instance()->player->position();

	const spk::Vector3 eye    { playerPos.x, playerPos.y, 20.0f };
	const spk::Vector3 center { playerPos.x, playerPos.y, 0.0f };
	const spk::Vector3 up     { 0.0f, 1.0f, 0.0f };

	const spk::Matrix4x4 view = spk::Matrix4x4::lookAt(eye, center, up);
	const spk::Matrix4x4 proj = _camera.projectionMatrix();

	_cameraUBO[L"view"] = view;
	_cameraUBO[L"proj"] = proj;
	_cameraUBO.validate();
}

void PlayerManager::_onGeometryChange()
{
	_screenRenderDimension = convertScreenToWorldPosition(geometry().size) - convertScreenToWorldPosition({0, 0});
	
	_camera.setOrthographic(-_screenRenderDimension.x / 2, _screenRenderDimension.x / 2, -_screenRenderDimension.y / 2, _screenRenderDimension.y / 2, 0, 100);

	_updatePlayerUBOs();
}

void PlayerManager::_onUpdateEvent(spk::UpdateEvent& p_event)
{
	if (Context::instance()->player == nullptr)
	{
		return ;
	}

	for (auto& [actionType, action] : _actions)
	{
		if (action->isInitialized() == false)
		{
			action->initialize(p_event);
		}

		if (action->isInitialized() == true)
		{
			action->update();
		}
	}
}

PlayerManager::PlayerManager(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	GraphicalWidget(p_name, p_parent),
	_cameraUBO(UniformBufferObjectAtlas::instance()->ubo(L"cameraData"))
{
	_actions[ActionType::MotionUp] = std::make_unique<spk::KeyboardAction>(spk::Keyboard::Z, spk::InputState::Down, 16, [&](){
				if (Context::instance()->player->isMoving() == false)
				{
					Context::instance()->player->move(spk::Vector2Int(0, 1));
				}
			});

	_actions[ActionType::MotionDown] = std::make_unique<spk::KeyboardAction>(spk::Keyboard::S, spk::InputState::Down, 16, [&](){
				if (Context::instance()->player->isMoving() == false)
				{
					Context::instance()->player->move(spk::Vector2Int(0, -1));
				}
			});

	_actions[ActionType::MotionRight] = std::make_unique<spk::KeyboardAction>(spk::Keyboard::D, spk::InputState::Down, 16, [&](){
				if (Context::instance()->player->isMoving() == false)
				{
					Context::instance()->player->move(spk::Vector2Int(1, 0));
				}
			});

	_actions[ActionType::MotionLeft] = std::make_unique<spk::KeyboardAction>(spk::Keyboard::Q, spk::InputState::Down, 16, [&](){
				if (Context::instance()->player->isMoving() == false)
				{
					Context::instance()->player->move(spk::Vector2Int(-1, 0));
				}
			});
}

void PlayerManager::initialize()
{
	Context::instance()->player = Context::instance()->actorMap.actor(0).upCast<Player>();

	_onPlayerEditionContract = Context::instance()->player->subscribeToEdition([&](){
		_updatePlayerUBOs();
		requestPaint();
	});
}