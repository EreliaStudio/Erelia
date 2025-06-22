#include "main_application.hpp"

void MainApplication::_onGeometryChange()
{
	WidgetAddons::centerInParent(&_mainMenu, spk::Vector2Int::clamp({200, 110}, geometry().size / 4, {500, 320}), geometry());

	_newGameMenu.setGeometry({0, geometry().size});
	_loadGameMenu.setGeometry({0, geometry().size});
	_gameMenu.setGeometry({0, geometry().size});
}

void MainApplication::_readConfigurationFile(const std::filesystem::path& p_path)
{
	_world->parse(spk::JSON::File(p_path));
}

void MainApplication::_createNewGame(const std::wstring& p_name, const std::wstring& p_seed)
{
	_world->name = p_name;
	_world->seed = p_seed;

	GameFile::instance()->save();

	_gameMenu.activate();
}

MainApplication::MainApplication(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::Widget(p_name, p_parent),
	_mainMenu(p_name + L"/MainMenu", this),
	_newGameMenu(p_name + L"/NewGameMenu", this),
	_loadGameMenu(p_name + L"/LoadGameMenu", this),
	_gameMenu(p_name + L"/GameMenu", this)
{
	_readConfigurationFile("resources/configuration.json");

	_mainMenu.activate();

	_mainMenu.onNewGameRequest([&](){
		_newGameMenu.activate();
	});
	
	_mainMenu.onLoadGameRequest([&](){
		_loadGameMenu.activate();
	});

	_mainMenu.onExitRequest([&](){
		_onQuitApplication();
	});

	_newGameMenu.onConfirmRequest([&](){
		_createNewGame(_newGameMenu.name(), _newGameMenu.seed());
	});

	_newGameMenu.onCancelRequest([&](){
		_mainMenu.activate();
	});
}

void MainApplication::setOnQuitApplication(const std::function<void()>& p_callback)
{
	_onQuitApplication = p_callback;
}