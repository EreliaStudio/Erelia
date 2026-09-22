#pragma once

#include <optional>
#include <string>

#include <sparkle>

namespace playground
{
	class FirstPersonController final : public spk::Behaviour
	{
	public:
		struct Configuration
		{
			float movementSpeed = 10.0f;
			float sprintMultiplier = 3.0f;
			float mouseSensitivity = 0.1f;
			float maximumPitch = 89.0f;
			spk::Keyboard::Key forwardKey = spk::Keyboard::Z;
			spk::Keyboard::Key backwardKey = spk::Keyboard::S;
			spk::Keyboard::Key leftKey = spk::Keyboard::Q;
			spk::Keyboard::Key rightKey = spk::Keyboard::D;
			spk::Keyboard::Key upwardKey = spk::Keyboard::Space;
			spk::Keyboard::Key downwardKey = spk::Keyboard::LeftControl;
			spk::Keyboard::Key sprintKey = spk::Keyboard::LeftShift;
			bool verticalMovementEnabled = true;
		};

	private:
		Configuration _configuration;
		float _yaw = 0.0f;
		float _pitch = 0.0f;
		bool _anglesInitialized = false;
		std::optional<spk::Vector2Int> _previousMouseCenter;

		[[nodiscard]] static Configuration _validated(Configuration configuration);
		[[nodiscard]] static spk::Vector3 _directionFromAngles(float yaw, float pitch) noexcept;
		[[nodiscard]] static spk::Vector3 _rotateVector(const spk::Quaternion &rotation, spk::Vector3 vector) noexcept;

		[[nodiscard]] spk::Entity3D &_owner3D();
		void _initializeAnglesFromOwnerRotation();
		[[nodiscard]] spk::Vector2Int _mouseCenter() const noexcept;

		void _applyMouseLook(spk::MouseMovedEvent &event, const spk::Vector2Int &mouseCenter);
		void _applyKeyboardMovement(const spk::Keyboard &keyboard, float deltaSeconds);

	protected:
		void _onMouseMovedEvent(spk::MouseMovedEvent &event) override;
		void _updateState(spk::UpdateContext &context) override;

	public:
		explicit FirstPersonController(const std::string &name = "First person controller");
		FirstPersonController(Configuration configuration, const std::string &name = "First person controller");

		void attach(spk::Entity *owner) override;

		[[nodiscard]] const Configuration &configuration() const noexcept;

		void setConfiguration(const Configuration &configuration);
	};
}
