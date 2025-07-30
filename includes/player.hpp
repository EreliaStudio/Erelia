#pragma once

#include "event.hpp"
#include "movable_entity.hpp"

class Player : public MovableEntity
{
private:
	class MotionInputController : public spk::Component
	{
	public:
		enum class ActionType
		{
			MoveUp,
			MoveDown,
			MoveRight,
			MoveLeft
		};

	private:
		EventDispatcher::Instanciator _eventDispatcherInstanciator;

		spk::SafePointer<MovableEntity::Behavior> _ownerMotionBehavior;

		std::unordered_map<ActionType, std::unique_ptr<spk::Action>> _actions;

		spk::SafePointer<spk::KeyboardAction> _moveUpAction;
		spk::SafePointer<spk::KeyboardAction> _moveDownAction;
		spk::SafePointer<spk::KeyboardAction> _moveRightAction;
		spk::SafePointer<spk::KeyboardAction> _moveLeftAction;

	protected:
		template <typename Enum>
		spk::SafePointer<spk::Action> setAction(Enum p_value, std::unique_ptr<spk::Action> &&action)
		{
			spk::SafePointer<spk::Action> result = action.get();
			_actions.emplace(p_value, std::move(action));
			return (result);
		}

	public:
		MotionInputController(const std::wstring &name);

		void onUpdateEvent(spk::UpdateEvent &p_event) override;

		void start() override;
	};

	class TopDownCamera : public spk::Component
	{
	private:
		spk::OpenGL::UniformBufferObject& _cameraUBO;

		spk::Camera _camera;
		spk::GameObject _cameraHolder;

		spk::ContractProvider::Contract _onEditionCallback;

	public:
		TopDownCamera(const std::wstring &p_name);

		void start() override;

		const spk::Matrix4x4& projectionMatrix() const;
		const spk::Matrix4x4& viewMatrix() const;
	};

	spk::SafePointer<MotionInputController> _controller;
	spk::SafePointer<TopDownCamera> _topDownCamera;

public:
	Player(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent);
};