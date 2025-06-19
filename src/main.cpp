#include <sparkle.hpp>

#include "utils/widget_override.hpp"

class MainMenu : public spk::Screen
{
private:
	spk::VerticalLayout _layout;
	PushButton _newGameButton;
	PushButton _loadGameButton;
	PushButton _exitButton;

	void _onGeometryChange()
	{
		_layout.setGeometry({0, geometry().size});
	}

public:
	MainMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
		spk::Screen(p_name, p_parent),
		_newGameButton(p_name + L"/NewGameButton", this),
		_loadGameButton(p_name + L"/LoadGameButton", this),
		_exitButton(p_name + L"/ExitButton", this)
	{
		_newGameButton.setText(L"New Game");
		_newGameButton.activate();

		_loadGameButton.setText(L"Load Game");
		_loadGameButton.activate();
		
		_exitButton.setText(L"Exit");
		_exitButton.activate();

		_layout.setElementPadding(10);

		_layout.addWidget(&_newGameButton);
		_layout.addWidget(&_loadGameButton);
		_layout.addWidget(&_exitButton);
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
	class Content : public spk::Widget
	{
	private:
		class SeedField : public spk::Widget
		{
		private:
			spk::HorizontalLayout _layout;
			TextEntry _field;
			PushButton _generateButton;

			void _onGeometryChange()
			{
				_generateButton.setMinimalSize(geometry().size);
				_layout.setGeometry(0, geometry().size);
			}

			void _generateRandomSeed()
			{
				_field.setText(std::to_wstring(rand() % 1000000));
			}

		public:
			SeedField(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
				spk::Widget(p_name, p_parent),
				_field(p_name + L"/Field", this),
				_generateButton(p_name + L"/GenerateButton", this)
			{
				_field.setPlaceholder(L"Enter seed value");
				_field.activate();

				_generateButton.setOnClick([&](){
					_generateRandomSeed();
				});

				_layout.setElementPadding(10);
				_layout.addWidget(&_field, SizePolicy::Extend);
				_layout.addWidget(&_generateButton, SizePolicy::Minimum);
			}

			spk::TextEntry& field()
			{
				return _field;
			}

			PushButton& generateButton()
			{
				return (_generateButton);
			}
		};

		spk::FormLayout _layout;

		spk::FormLayout::Row<spk::TextEntry> _nameRow;
		spk::FormLayout::Row<spk::TextEntry> _seedRow;
		spk::SpacerWidget _spacer;

		void _onGeometryChange()
		{
			_layout.setGeometry({0, geometry().size});
		}

	public:
		Content(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
			spk::Widget(p_name, p_parent),
			_nameRow(p_name + L"/NameRow", this),
			_seedRow(p_name + L"/SeedRow", this),
			_spacer(p_name + L"/Spacer", this)
		{
			WidgetAddons::ApplyFormat(&_nameRow.label);
			WidgetAddons::ApplyFormat(&_nameRow.field);
			WidgetAddons::ApplyFormat(&_seedRow.label);
			WidgetAddons::ApplyFormat(&_seedRow.field);

			_nameRow.label.setText(L"Name:");
			_nameRow.field.setText(L"New game");
			_nameRow.activate();

			_seedRow.label.setText(L"Seed:");
			_seedRow.field.setPlaceholder(L"Empty for random seed");
			_seedRow.activate();

			_layout.setElementPadding(10);
			_layout.addRow(&_nameRow, SizePolicy::Minimum, SizePolicy::HorizontalExtend);
			_layout.addRow(&_seedRow, SizePolicy::Minimum, SizePolicy::HorizontalExtend);
			_layout.addRow(nullptr, &_spacer, SizePolicy::Minimum, SizePolicy::Extend);
		}
	};

	spk::VerticalLayout _layout;

	Content _content;
	CommandPanel _commandPanel;
	spk::SafePointer<spk::PushButton> _confirmButton;
	spk::SafePointer<spk::PushButton> _cancelButton;

	void _onGeometryChange()
	{
		_layout.setGeometry({0, geometry().size});
	}

public:
	NewGameMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
		spk::Screen(p_name, p_parent),
		_commandPanel(p_name + L"/CommandPanel", this),
		_content(p_name + L"/Content", this)
	{
		_content.activate();

		_confirmButton = _commandPanel.addButton(L"Confirm", L"Confirm", [this]() {});

		_cancelButton = _commandPanel.addButton(L"Cancel", L"Cancel", [this]() {});

		WidgetAddons::ApplyFormat(_confirmButton);
		WidgetAddons::ApplyFormat(_cancelButton);

		_commandPanel.activate();

		_layout.setElementPadding(10);
		_layout.addWidget(&_content);
		_layout.addWidget(&_commandPanel);
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

	void _onGeometryChange()
	{
		WidgetAddons::centerInParent(&_mainMenu, spk::Vector2Int::clamp({200, 110}, geometry().size / 4, {500, 320}), geometry());
		_newGameMenu.setGeometry({0, geometry().size});
		_loadGameMenu.setGeometry({0, geometry().size});
	}

public:
	MainWidget(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
		spk::Widget(p_name, p_parent),
		_mainMenu(p_name + L"/MainMenu", this),
		_newGameMenu(p_name + L"/NewGameMenu", this),
		_loadGameMenu(p_name + L"/LoadGameMenu", this)
	{
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
	}

	void setOnQuitApplication(const std::function<void()>& p_callback)
	{
		_onQuitApplication = p_callback;
	}
};

int main()
{
	spk::GraphicalApplication app;

	spk::SafePointer<spk::Window> win = app.createWindow(L"Erelia", {{0, 0}, {800, 600}});

	MainWidget mainMenu = MainWidget(L"MainWidget", win->widget());
	mainMenu.setOnQuitApplication([&](){app.quit(0);});
	mainMenu.setGeometry({0, win->geometry().size});
	mainMenu.activate();

	return (app.run());
}