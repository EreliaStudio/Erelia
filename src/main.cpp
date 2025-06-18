#include <sparkle.hpp>

#include "utils/widget_override.hpp"

class MainMenu : public spk::Screen
{
private:
	spk::VerticalLayout _layout;
	PushButton _newGameButton;
	PushButton _loadGameButton;
	PushButton _exitButton;

public:
	MainMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
		spk::Screen(p_name, p_parent),
		_newGameButton(p_name + L"/New Game", this),
		_loadGameButton(p_name + L"/Load Game", this),
		_exitButton(p_name + L"/Exit", this)
	{
		_newGameButton.setText(L"New Game");
		_newGameButton.activate();

		_loadGameButton.setText(L"Load Game");
		_loadGameButton.activate();
		
		_exitButton.setText(L"Exit");
		_exitButton.activate();
	}

	void onNewGameRequest(const spk::PushButton::Job& p_job)
	{
		_newGameButton.setOnClick(p_job);
	}

	void onLoadGameRequest(const spk::PushButton::Job& p_job)
	{
		_loadGameButton.setOnClick(p_job);
	}

	void onExitRequest(const spk::PushButton::Job& p_job)
	{
		_exitButton.setOnClick(p_job);
	}
};

class NewGameMenu : public spk::Screen
{
private:
	CommandPanel _commandPanel;
	spk::SafePointer<spk::PushButton> _confirmButton;
	spk::SafePointer<spk::PushButton> _cancelButton;

public:
	NewGameMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
		spk::Screen(p_name, p_parent),
		_commandPanel(p_name + L"/CommandPanel", this)
	{
		_commandPanel.addButton(L"Confirm", L"Confirm", [this]() {

		});

		_commandPanel.addButton(L"Cancel", L"Cancel", [this]() {

		});
	}

	void onConfirmRequest(const spk::PushButton::Job& p_job)
	{
		_commandPanel.setOnClick(L"Confirm", p_job);
	}

	void onCancelRequest(const spk::PushButton::Job& p_job)
	{
		_commandPanel.setOnClick(L"Cancel", p_job);
	}
};

class LoadGameMenu : public spk::Screen
{
private:
	PushButton _cancelButton;

public:
	LoadGameMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
		spk::Screen(p_name, p_parent),
		_cancelButton(p_name + L"/Cancel", this)
	{
		_cancelButton.setText(L"Cancel");
		_cancelButton.activate();
	}

	void onCancelRequest(const spk::PushButton::Job& p_job)
	{
		_cancelButton.setOnClick(p_job);
	}
};

class MainWidget : public spk::Widget
{
private:
	MainMenu _mainMenu;
	NewGameMenu _newGameMenu;
	LoadGameMenu _loadGameMenu;

	std::function<void()> _onQuitApplication;

public:
	MainWidget(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
		spk::Widget(p_name, p_parent),
		_mainMenu(p_name + L"/MainMenu", this),
		_newGameMenu(p_name + L"/NewGameMenu", this),
		_loadGameMenu(p_name + L"/LoadGameMenu", this)
	{
		_mainMenu.newGameButton().setOnClick([&](){
			_newGameMenu.activate();
		})
		
		_mainMenu.loadGameButton().setOnClick([&](){
			_loadGameMenu.activate();
		})

		_mainMenu.exitButton().setOnClick([&](){
			_onQuitApplication();
		});
	}

	void setOnQuitApplication(const std::function<void()>& p_callback)
	{
		_onQuitApplication = p_callback;
	}
}

int main()
{
	spk::GraphicalApplication app;

	spk::SafePointer<spk::Window> win = app.createWindow(L"Erelia", {{0, 0}, {800, 600}});

	MainMenu mainMenu = MainMenu(L"MainMenu", win->widget());
	mainMenu.setOnQuitApplication([&](){app.quit();});
	mainMenu.setGeometry({0, win->geometry().size});
	mainMenu.activate();

	return (app.run());
}