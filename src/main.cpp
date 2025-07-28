#include <sparkle.hpp>
#include <spk_debug_macro.hpp>

enum class Event
{
	MovePlayer
};

using EventDispatcher = spk::EventDispatcher<Event>;

class Controller : public spk::Component
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

	std::unordered_map<ActionType, std::unique_ptr<spk::Action>> _actions;

	spk::OpenGL::UniformBufferObject& _cameraUBO;

	static spk::OpenGL::UniformBufferObject& requestCameraUBO()
	{
		if (spk::Lumina::Shader::Constants::containsUBO(L"cameraUBO") == false)
		{
			spk::OpenGL::UniformBufferObject newUBO = spk::OpenGL::UniformBufferObject(L"CameraUBO", 0, 128);

			newUBO.addElement(L"viewMatrix", 0, 64);
			newUBO.addElement(L"projectionMatrix", 0, 64);

			spk::Lumina::Shader::Constants::addUBO(L"cameraUBO", std::move(newUBO));
		}
		return (spk::Lumina::Shader::Constants::ubo(L"cameraUBO"));
	}

	spk::SafePointer<spk::KeyboardAction> _moveUpAction;
	spk::SafePointer<spk::KeyboardAction> _moveDownAction;
	spk::SafePointer<spk::KeyboardAction> _moveRightAction;
	spk::SafePointer<spk::KeyboardAction> _moveLeftAction;

	spk::Camera _camera;
	spk::GameObject _cameraHolder;

	spk::ContractProvider::Contract _onEditionCallback;

protected:
	template <typename Enum>
	spk::SafePointer<spk::Action> setAction(Enum p_value, std::unique_ptr<spk::Action> &&action)
	{
		spk::SafePointer<spk::Action> result = action.get();
		_actions.emplace(p_value, std::move(action));
		return (result);
	}

	void onUpdateEvent(spk::UpdateEvent &p_event)
	{
		
		for (auto &[key, action] : _actions)
		{
			if (action->isInitialized() == false)
			{
				action->initialize(p_event);
			}

			action->update();
		}
	}

public:
	Controller(const std::wstring &name) :
		spk::Component(name),
		_cameraHolder(L"CameraHolder", nullptr),
		_cameraUBO(requestCameraUBO())
	{
		_cameraHolder.transform().place({0, 0, 10});
		_cameraHolder.transform().lookAt({0, 0, 0});

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

	void awake()
	{
		owner()->addChild(&_cameraHolder);

		_onEditionCallback = owner()->transform().addOnEditionCallback([&](){
			_cameraUBO[L"viewMatrix"] = viewMatrix();
			_cameraUBO[L"projectionMatrix"] = projectionMatrix();
			_cameraUBO.validate();
		});
	}

	const spk::Matrix4x4& projectionMatrix() const
	{
		return (_camera.projectionMatrix());
	}

	const spk::Matrix4x4& viewMatrix() const
	{
		return (_cameraHolder.transform().inverseModel());
	}
};

class Player : public spk::GameObject
{
private:
	Controller& _controller;

public:
	Player(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent) :
		spk::GameObject(p_name, p_parent),
		_controller(addComponent<Controller>(L"Controller"))
	{
		_controller.activate();
	}
};

class World : public spk::GameObject
{
private:

public:
	World(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent) :
		spk::GameObject(p_name, p_parent)
	{

	}
};

struct PlayerManager : public spk::GameObject
{
private:
	EventDispatcher::Instanciator _eventDispatcherInstanciator;

public:
	PlayerManager(const std::wstring &p_name, spk::SafePointer<spk::GameObject> p_parent) :
		spk::GameObject(p_name, p_parent)
	{

	}
};

struct Context
{
	spk::GameEngine engine;

	Player player;
	World world;

	Context() :
		player(L"Player", nullptr),
		world(L"World", nullptr)
	{
		player.transform().place({0, 0, 0});
		player.activate();

		world.activate();
		
		engine.addEntity(&player);
		engine.addEntity(&world);
	}
};

class TestWidget : public spk::Screen
{
private:
	spk::GameEngineWidget _gameEngineWidget;

	Context _context;

	void _onGeometryChange()
	{
		_gameEngineWidget.setGeometry({}, geometry().size);
	}

public:
	TestWidget(const std::wstring &p_name, spk::SafePointer<spk::Widget> p_parent) :
		spk::Screen(p_name, p_parent),
		_gameEngineWidget(p_name + L"/GameEngineWidget", this)
	{
		_gameEngineWidget.setGameEngine(&(_context.engine));
		_gameEngineWidget.activate();
	}
};

int main()
{
	spk::GraphicalApplication app;

	spk::SafePointer<spk::Window> win = app.createWindow(L"Erelia", {{0, 0}, {800, 600}});

	TestWidget testWidget = TestWidget(L"TestWidget", win->widget());
	testWidget.setGeometry({0, 0}, win->geometry().size);
	testWidget.activate();

	return (app.run());
}