#include "structure/actor.hpp"

void Actor::update()
{
	if (isMoving() == true)
	{
		_position = spk::Vector2::lerp(_origin, _destination, _motionTimer.elapsedRatio());
		_onEditionContractProvider.trigger();
	}
	else if (_motionTimer.state() != spk::Timer::State::TimedOut)
	{
		_position = _destination;
		_motionTimer.stop();
		_onEditionContractProvider.trigger();
	}
}

spk::ContractProvider::Contract Actor::subscribeToEdition(const spk::ContractProvider::Job& p_job)
{
	return (_onEditionContractProvider.subscribe(p_job));
}

bool Actor::isMoving() const
{
	return (_motionTimer.state() == spk::Timer::State::Running);
}

void Actor::move(const spk::Vector2& p_delta)
{
	_position = _destination;
	_origin = _position;
	_destination = _origin + p_delta;
	_motionTimer.start();
}

void Actor::place(const spk::Vector2& p_position)
{
	_position = p_position;
	_motionTimer.stop();
}

const spk::Vector2& Actor::position() const
{
	return _position;
}