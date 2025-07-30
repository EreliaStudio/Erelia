#include "player.hpp"

Player::Player(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent) :
	MovableEntity(p_name, p_parent),
	_controller(addComponent<MotionInputController>(p_name + L"/MotionInputController")),
	_topDownCamera(addComponent<TopDownCamera>(p_name + L"/TopDownCamera"))
{
	_controller->activate();
	_topDownCamera->activate();
}