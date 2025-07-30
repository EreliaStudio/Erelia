#include "player.hpp"

Player::MotionInputController::MotionInputController(const std::wstring &name) :
	spk::Component(name)
{
	_moveUpAction = setAction(ActionType::MoveUp, std::make_unique<spk::KeyboardAction>(spk::Keyboard::Z, spk::InputState::Down, 26,
		[&](const spk::SafePointer<const spk::Keyboard>& p_keyboard) {
			EventDispatcher::instance()->emit(Event::MovePlayer, spk::Vector3(0, 1, 0));
		})).upCast<spk::KeyboardAction>();

	_moveDownAction = setAction(ActionType::MoveDown, std::make_unique<spk::KeyboardAction>(spk::Keyboard::S, spk::InputState::Down, 26,
		[&](const spk::SafePointer<const spk::Keyboard>& p_keyboard) {
			EventDispatcher::instance()->emit(Event::MovePlayer, spk::Vector3(0, -1, 0));
		})).upCast<spk::KeyboardAction>();

	_moveRightAction = setAction(ActionType::MoveRight, std::make_unique<spk::KeyboardAction>(spk::Keyboard::Q, spk::InputState::Down, 26,
		[&](const spk::SafePointer<const spk::Keyboard>& p_keyboard) {
			EventDispatcher::instance()->emit(Event::MovePlayer, spk::Vector3(1, 0, 0));
		})).upCast<spk::KeyboardAction>();

	_moveLeftAction = setAction(ActionType::MoveLeft, std::make_unique<spk::KeyboardAction>(spk::Keyboard::D, spk::InputState::Down, 26,
		[&](const spk::SafePointer<const spk::Keyboard>& p_keyboard) {
			EventDispatcher::instance()->emit(Event::MovePlayer, spk::Vector3(-1, 0, 0));
		})).upCast<spk::KeyboardAction>();
}

void Player::MotionInputController::onUpdateEvent(spk::UpdateEvent &p_event)
{
	if (_ownerMotionBehavior->isMoving() == true)
	{
		return ;
	}

	for (auto &[key, action] : _actions)
	{
		if (action->isInitialized() == false)
		{
			action->initialize(p_event);
		}

		action->update();
	}
}

void Player::MotionInputController::awake()
{
	if (owner() == nullptr)
	{
		GENERATE_ERROR("MotionInputController must be attached to a GameObject.");
	}
	
	_ownerMotionBehavior = owner()->getComponent<MovableEntity::Behavior>(L"Behavior");
	
	if (_ownerMotionBehavior == nullptr)
	{
		GENERATE_ERROR("MotionInputController must be attached to a MovableEntity.");
	}
}
