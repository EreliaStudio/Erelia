#include "movable_entity.hpp"

MovableEntity::Behavior::Behavior(const std::wstring &p_name) : spk::Component(p_name)
{
}

void MovableEntity::Behavior::onUpdateEvent(spk::UpdateEvent &p_event)
{
	if (_motionTimer.state() != spk::Timer::State::Running)
	{
		return;
	}

	spk::Vector3 newPosition = spk::Vector3::lerp(_origin, _destination, _motionTimer.elapsedRatio());

	owner()->transform().place(newPosition);
}

bool MovableEntity::Behavior::isMoving() const
{
	return (_motionTimer.state() == spk::Timer::State::Running);
}

void MovableEntity::Behavior::setMotionDuration(const spk::Duration &p_duration)
{
	_motionTimer = spk::Timer(p_duration);
}

void MovableEntity::Behavior::move(const spk::Vector3 &p_delta)
{
	_origin = owner()->transform().position();
	_destination = _origin + p_delta;

	if (_motionTimer.state() != spk::Timer::State::Running)
	{
		_motionTimer.start();
	}
}

void MovableEntity::Behavior::place(const spk::Vector3 &p_position)
{
	_motionTimer.stop();
	owner()->transform().place(p_position);
}

MovableEntity::MovableEntity(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent) :
	spk::GameObject(p_name, p_parent)
{
	_behavior = addComponent<Behavior>(L"Behavior");
}

void MovableEntity::move(const spk::Vector3 &p_delta)
{
	_behavior->move(p_delta);
}

void MovableEntity::place(const spk::Vector3 &p_position)
{
	_behavior->place(p_position);
}