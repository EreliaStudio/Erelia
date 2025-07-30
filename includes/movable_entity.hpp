#pragma once

#include <sparkle.hpp>

class MovableEntity : public spk::GameObject
{
public:
	class Behavior : public spk::Component
	{
	private:
		spk::Timer _motionTimer;
		spk::Vector3 _origin;
		spk::Vector3 _destination;

	public:
		Behavior(const std::wstring &p_name);

		void onUpdateEvent(spk::UpdateEvent &p_event) override;

		bool isMoving() const;

		void setMotionDuration(const spk::Duration& p_duration);

		void move(const spk::Vector3& p_delta);
		void place(const spk::Vector3& p_position);
	};

private:
	spk::SafePointer<Behavior> _behavior;

public:
	MovableEntity(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent);

	Behavior& behavior() { return *_behavior; }
	const Behavior& behavior() const { return *_behavior; }
};
