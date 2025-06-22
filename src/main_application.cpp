#include "main_application.hpp"

void MainApplication::_onGeometryChange()
{
	WidgetAddons::centerInParent(&_mainMenu, spk::Vector2Int::clamp({200, 110}, geometry().size / 4, {500, 320}), geometry());

	_newGameMenu.setGeometry({0, geometry().size});
	_loadGameMenu.setGeometry({0, geometry().size});
	_gameMenu.setGeometry({0, geometry().size});
}

void MainApplication::_createNewGame(const std::wstring& p_name, const std::wstring& p_seed, const spk::Vector2UInt& p_iconSprite)
{
	GameFile::createNewGameFile(p_name, p_seed, p_iconSprite);

	_gameMenu.activate();
}

MainApplication::MainApplication(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::Widget(p_name, p_parent),
	_mainMenu(p_name + L"/MainMenu", this),
	_newGameMenu(p_name + L"/NewGameMenu", this),
	_loadGameMenu(p_name + L"/LoadGameMenu", this),
	_gameMenu(p_name + L"/GameMenu", this)
{
	GameFile::configure(spk::JSON::File("resources/configuration.json"));

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
		_createNewGame(_newGameMenu.name(), _newGameMenu.seed(), _newGameMenu.iconSprite());
	});

	_newGameMenu.onCancelRequest([&](){
		_mainMenu.activate();
	});
}

void MainApplication::setOnQuitApplication(const std::function<void()>& p_callback)
{
	_onQuitApplication = p_callback;
}