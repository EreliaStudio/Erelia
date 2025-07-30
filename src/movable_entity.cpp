#include "movable_entity.hpp"

MovableEntity::Behavior::Behavior(const std::wstring &p_name) :
	spk::Component(p_name)
{
	setMotionDuration(spk::Duration(50, spk::TimeUnit::Millisecond));
}

void MovableEntity::Behavior::onUpdateEvent(spk::UpdateEvent &p_event)
{
	switch (_motionTimer.state())
	{
	case spk::Timer::State::TimedOut:
			place(_destination);
	case spk::Timer::State::Idle:
			return;
	}
	
	owner()->transform().place(spk::Vector3::lerp(_origin, _destination, _motionTimer.elapsedRatio()));
	spk::cout << "New [" << owner()->name() << "] position : " << owner()->transform().position() << std::endl;
}

bool MovableEntity::Behavior::isMoving() const
{
	return (_motionTimer.state() != spk::Timer::State::Idle);
}

void MovableEntity::Behavior::setMotionDuration(const spk::Duration &p_duration)
{
	_motionTimer = spk::Timer(p_duration);
}

void MovableEntity::Behavior::move(const spk::Vector3 &p_delta)
{
	if (_motionTimer.state() == spk::Timer::State::Running)
	{
		return;
	}

	spk::cout << "Moving [" << owner()->name() << "] from " << owner()->transform().position() << " by " << p_delta << std::endl;
	_origin = owner()->transform().position();
	_destination = _origin + p_delta;
	_motionTimer.start();
}

void MovableEntity::Behavior::place(const spk::Vector3 &p_position)
{
	_motionTimer.stop();
	owner()->transform().place(p_position);
	_origin = p_position;
	_destination = p_position;
	spk::cout << "Fixing [" << owner()->name() << "] position : " << owner()->transform().position() << std::endl;
}

MovableEntity::MovableEntity(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent) :
	spk::GameObject(p_name, p_parent)
{
	_behavior = addComponent<Behavior>(L"Behavior");
}