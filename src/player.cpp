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

void Player::MotionInputController::onUpdateEvent(spk::UpdateEvent &p_event) override
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

void Player::MotionInputController::awake() override
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

Player::TopDownCamera::TopDownCamera(const std::wstring &p_name) :
	spk::Component(p_name),
	_cameraHolder(p_name + L"/CameraHolder", nullptr),
	_cameraUBO(UBOFactory::cameraUBO())
{
	_cameraHolder.transform().place({0, 0, 10});
	_cameraHolder.transform().lookAt({0, 0, 0});
}

void Player::TopDownCamera::awake() override
{
	owner()->addChild(&_cameraHolder);

	_onEditionCallback = owner()->transform().addOnEditionCallback([&](){
		_cameraUBO[L"viewMatrix"] = viewMatrix();
		_cameraUBO[L"projectionMatrix"] = projectionMatrix();
		_cameraUBO.validate();
	});
}

const spk::Matrix4x4& Player::TopDownCamera::projectionMatrix() const
{
	return (_camera.projectionMatrix());
}

const spk::Matrix4x4& Player::TopDownCamera::viewMatrix() const
{
	return (_cameraHolder.transform().inverseModel());
}

Player::Player(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent) :
	MovableEntity(p_name, p_parent),
	_controller(addComponent<MotionInputController>(p_name + L"/MotionInputController")),
	_topDownCamera(addComponent<TopDownCamera>(p_name + L"/TopDownCamera"))
{
	_controller.activate();
	_topDownCamera.activate();
}