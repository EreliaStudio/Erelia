#include "behaviour/first_person_controller.hpp"

#include <algorithm>
#include <chrono>
#include <cmath>
#include <numbers>

#include <exception.hpp>

namespace playground
{
	namespace
	{
		constexpr float RadiansToDegrees = 180.0f / std::numbers::pi_v<float>;

		spk::Vector3 scaled(spk::Vector3 vector, float factor) noexcept
		{
			return {vector.x * factor, vector.y * factor, vector.z * factor};
		}
	}

	FirstPersonController::FirstPersonController(const std::string &name) :
		FirstPersonController(Configuration{}, name)
	{
	}

	FirstPersonController::FirstPersonController(Configuration configuration, const std::string &name) :
		spk::Behaviour(name),
		_configuration(_validated(configuration))
	{
	}

	void FirstPersonController::attach(spk::Entity *owner)
	{
		if (owner != nullptr && dynamic_cast<spk::Entity3D *>(owner) == nullptr)
		{
			throw spk::Exception("FirstPersonController can only be attached to an Entity3D");
		}
		spk::Behaviour::attach(owner);
		_anglesInitialized = false;
		_previousMouseCenter.reset();
	}

	void FirstPersonController::setConfiguration(const Configuration &configuration)
	{
		_configuration = _validated(configuration);
	}

	const FirstPersonController::Configuration &FirstPersonController::configuration() const noexcept
	{
		return _configuration;
	}

	FirstPersonController::Configuration FirstPersonController::_validated(Configuration configuration)
	{
		if (configuration.movementSpeed < 0.0f)
		{
			throw spk::Exception("FirstPersonController movement speed cannot be negative");
		}
		if (configuration.sprintMultiplier < 0.0f)
		{
			throw spk::Exception("FirstPersonController sprint multiplier cannot be negative");
		}
		if (configuration.mouseSensitivity < 0.0f)
		{
			throw spk::Exception("FirstPersonController mouse sensitivity cannot be negative");
		}
		if (configuration.maximumPitch < 0.0f || configuration.maximumPitch >= 90.0f)
		{
			throw spk::Exception("FirstPersonController maximum pitch must be in [0, 90)");
		}
		return configuration;
	}

	spk::Vector3 FirstPersonController::_directionFromAngles(float yaw, float pitch) noexcept
	{
		const float yawRadians = yaw / RadiansToDegrees;
		const float pitchRadians = pitch / RadiansToDegrees;
		const float horizontalLength = std::cos(pitchRadians);
		return {
			std::sin(yawRadians) * horizontalLength,
			std::sin(pitchRadians),
			-std::cos(yawRadians) * horizontalLength};
	}

	spk::Vector3 FirstPersonController::_rotateVector(const spk::Quaternion &rotation, spk::Vector3 vector) noexcept
	{
		const spk::Vector3 quaternionVector{rotation.x, rotation.y, rotation.z};
		const spk::Vector3 twiceCross = scaled(quaternionVector.cross(vector), 2.0f);
		return vector + scaled(twiceCross, rotation.w) + quaternionVector.cross(twiceCross);
	}

	spk::Entity3D &FirstPersonController::_owner3D()
	{
		auto *entity = dynamic_cast<spk::Entity3D *>(owner());
		if (entity == nullptr)
		{
			throw spk::Exception("FirstPersonController is not attached to an Entity3D");
		}
		return *entity;
	}

	void FirstPersonController::_initializeAnglesFromOwnerRotation()
	{
		const auto forward = _rotateVector(_owner3D().transform().rotation(), {0.0f, 0.0f, -1.0f}).normalized();
		_pitch = std::asin(std::clamp(forward.y, -1.0f, 1.0f)) * RadiansToDegrees;
		_yaw = std::atan2(forward.x, -forward.z) * RadiansToDegrees;
		_anglesInitialized = true;
	}

	spk::Vector2Int FirstPersonController::_mouseCenter() const noexcept
	{
		return {
			geometry().anchor.x + static_cast<spk::Vector2Int::value_type>(geometry().size.x / 2),
			geometry().anchor.y + static_cast<spk::Vector2Int::value_type>(geometry().size.y / 2)};
	}

	void FirstPersonController::_applyMouseLook(spk::MouseMovedEvent &event, const spk::Vector2Int &mouseCenter)
	{
		if (!_previousMouseCenter.has_value() || *_previousMouseCenter != mouseCenter)
		{
			_previousMouseCenter = mouseCenter;
			if (event.record.position != mouseCenter)
			{
				event.requestMousePosition(mouseCenter);
			}
			return;
		}
		const spk::Vector2Int mouseMovement = event.record.position - mouseCenter;
		if (mouseMovement == spk::Vector2Int{})
		{
			return;
		}
		event.requestMousePosition(mouseCenter);

		_yaw += static_cast<float>(mouseMovement.x) * _configuration.mouseSensitivity;
		_pitch -= static_cast<float>(mouseMovement.y) * _configuration.mouseSensitivity;
		_pitch = std::clamp(_pitch, -_configuration.maximumPitch, _configuration.maximumPitch);

		auto &transform = _owner3D().transform();
		const auto forward = _directionFromAngles(_yaw, _pitch);
		const auto position = transform.position();
		transform.setRotation(spk::Quaternion::lookAt(position, position + forward));
	}

	void FirstPersonController::_onMouseMovedEvent(spk::MouseMovedEvent &event)
	{
		_applyMouseLook(event, _mouseCenter());
	}

	void FirstPersonController::_applyKeyboardMovement(const spk::Keyboard &keyboard, float deltaSeconds)
	{
		const auto pressed = [&](spk::Keyboard::Key key) {
			return keyboard[key] == spk::InputState::Down;
		};
		const auto forward = _directionFromAngles(_yaw, _pitch);
		const auto right = forward.cross({0.0f, 1.0f, 0.0f}).normalized();
		spk::Vector3 movement{};
		if (pressed(_configuration.forwardKey))
		{
			movement += forward;
		}
		if (pressed(_configuration.backwardKey))
		{
			movement -= forward;
		}
		if (pressed(_configuration.leftKey))
		{
			movement -= right;
		}
		if (pressed(_configuration.rightKey))
		{
			movement += right;
		}
		if (_configuration.verticalMovementEnabled)
		{
			if (pressed(_configuration.upwardKey))
			{
				movement.y += 1.0f;
			}
			if (pressed(_configuration.downwardKey))
			{
				movement.y -= 1.0f;
			}
		}
		if (movement.length() == 0.0)
		{
			return;
		}

		float speed = _configuration.movementSpeed;
		if (pressed(_configuration.sprintKey))
		{
			speed *= _configuration.sprintMultiplier;
		}
		_owner3D().transform().move(scaled(movement.normalized(), speed * deltaSeconds));
	}

	void FirstPersonController::_updateState(spk::UpdateContext &context)
	{
		if (!_anglesInitialized)
		{
			_initializeAnglesFromOwnerRotation();
		}
		const float deltaSeconds = std::chrono::duration<float>(context.deltaTime).count();
		if (deltaSeconds > 0.0f)
		{
			_applyKeyboardMovement(context.keyboard, deltaSeconds);
		}
	}
}
