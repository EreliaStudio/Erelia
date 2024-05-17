#include "sparkle.hpp"

#include "miscellaneous/texture_manager.hpp"
#include "widget/widget_redefinition.hpp"

class MainMenuWidget : public spk::Widget
{
private:
	Frame _backgroundFrame;

	TextLabel _gameNameTextLabel;

	Button _playButton;
	Button _settingsButton;
	Button _quitButton;

	void _onUpdate() override
	{
		
	}

	size_t computeButtonTextSize(const spk::Vector2Int &p_desiredArea)
	{
		size_t buttonTextSizes[3] = {
			_playButton.label(spk::Button::State::Released).computeOptimalTextSize(p_desiredArea),
			_settingsButton.label(spk::Button::State::Released).computeOptimalTextSize(p_desiredArea),
			_quitButton.label(spk::Button::State::Released).computeOptimalTextSize(p_desiredArea)
		};

		size_t result = 25;

		for (size_t i = 0; i < 3; i++)
		{
			result = std::min(result, buttonTextSizes[i]);
		}

		return (result);
	}

	void _onGeometryChange() override
	{
		_backgroundFrame.setGeometry(0, size());

		spk::Vector2 gameNameTextLabelAnchor = spk::Vector2(10.0f + _backgroundFrame.box().cornerSize().x, 10.0f + _backgroundFrame.box().cornerSize().y);
		spk::Vector2 gameNameTextLabelSize = spk::Vector2(
			size().x - (20.0f + _backgroundFrame.box().cornerSize().x * 2),
			(size().y - (20.0f + _backgroundFrame.box().cornerSize().y * 2)) / 2.0f - 10);

		_gameNameTextLabel.box().setCornerSize(std::min(gameNameTextLabelSize.x * 0.1f, gameNameTextLabelSize.y * 0.1f));
		_gameNameTextLabel.setGeometry(gameNameTextLabelAnchor, gameNameTextLabelSize);
		size_t gameNameTextLabelTextSize = _gameNameTextLabel.label().computeOptimalTextSize(gameNameTextLabelSize - _gameNameTextLabel.box().cornerSize() * 2);
		size_t gameNameTextLabelOutlineSize = std::min(6.0f, gameNameTextLabelTextSize * 0.2f);
		gameNameTextLabelTextSize -= gameNameTextLabelOutlineSize * 2;
		_gameNameTextLabel.label().setTextSize(gameNameTextLabelTextSize);
		_gameNameTextLabel.label().setOutlineSize(gameNameTextLabelOutlineSize);

		spk::Vector2 buttonAnchor = gameNameTextLabelAnchor + spk::Vector2(0, gameNameTextLabelSize.y + 10);
		spk::Vector2 buttonSize = spk::Vector2(gameNameTextLabelSize.x, (size().y - gameNameTextLabelSize.y - (10.0f + _backgroundFrame.box().cornerSize().y * 2) - 40) / 3.0f);
		size_t buttonTextSize = computeButtonTextSize(buttonSize);

		_playButton.setGeometry(buttonAnchor + (buttonSize + spk::Vector2(0, 10.0f)) * spk::Vector2(0.0f, 0.0f), buttonSize);
		_playButton.label(spk::Button::State::Pressed).setTextSize(buttonTextSize);
		_playButton.label(spk::Button::State::Released).setTextSize(buttonTextSize);

		_settingsButton.setGeometry(buttonAnchor + (buttonSize + spk::Vector2(0, 10.0f)) * spk::Vector2(0.0f, 1.0f), buttonSize);
		_settingsButton.label(spk::Button::State::Pressed).setTextSize(buttonTextSize);
		_settingsButton.label(spk::Button::State::Released).setTextSize(buttonTextSize);

		_quitButton.setGeometry(buttonAnchor + (buttonSize + spk::Vector2(0, 10.0f)) * spk::Vector2(0.0f, 2.0f), buttonSize);
		_quitButton.label(spk::Button::State::Pressed).setTextSize(buttonTextSize);
		_quitButton.label(spk::Button::State::Released).setTextSize(buttonTextSize);
	}

	void _onRender() override
	{

	}

public:
	MainMenuWidget(spk::Widget* p_parent) :
		MainMenuWidget("Unnamed MainMenuWidget", p_parent)
	{

	}

	MainMenuWidget(const std::string& p_name, spk::Widget* p_parent) :
		spk::Widget(p_name, p_parent),
		_backgroundFrame("BackgroundFrame", this),
		_gameNameTextLabel("GameNameTextLabel", &_backgroundFrame),
		_playButton("PlayButton", &_backgroundFrame),
		_settingsButton("SettingsButton", &_backgroundFrame),
		_quitButton("QuitButton", &_backgroundFrame)
	{
		_backgroundFrame.activate();

		_gameNameTextLabel.box().setSpriteSheet(TextureManager::instance()->spriteSheet("GameNameFrame"));
		_gameNameTextLabel.label().setText("Erelia");
		_gameNameTextLabel.label().setFont(TextureManager::instance()->font("VIKING-N"));
		_gameNameTextLabel.label().setHorizontalAlignment(spk::HorizontalAlignment::Centered);
		_gameNameTextLabel.label().setVerticalAlignment(spk::VerticalAlignment::Centered);
		_gameNameTextLabel.activate();

		_playButton.label(spk::Button::State::Pressed).setText("Play");
		_playButton.label(spk::Button::State::Released).setText("Play");
		_playButton.label(spk::Button::State::Pressed).setOutlineSize(1);
		_playButton.label(spk::Button::State::Released).setOutlineSize(1);
		_playButton.activate();
		
		_settingsButton.label(spk::Button::State::Pressed).setText("Settings");
		_settingsButton.label(spk::Button::State::Released).setText("Settings");
		_settingsButton.label(spk::Button::State::Pressed).setOutlineSize(1);
		_settingsButton.label(spk::Button::State::Released).setOutlineSize(1);
		_settingsButton.activate();

		_quitButton.label(spk::Button::State::Pressed).setText("Quit");
		_quitButton.label(spk::Button::State::Released).setText("Quit");
		_quitButton.label(spk::Button::State::Pressed).setOutlineSize(1);
		_quitButton.label(spk::Button::State::Released).setOutlineSize(1);
		_quitButton.activate();
	}
};

class MainMenuPanel : public spk::Panel
{
private:
    spk::ImageLabel _backgroundLabel;
    spk::ImageLabel _riverLabel;
    spk::ImageLabel _treeLabel;
    spk::ImageLabel _cloudLabel;

	MainMenuWidget _mainMenu;

    void _onUpdate() override
    {
        
    }

    void _onGeometryChange() override
    {
        _backgroundLabel.setGeometry(0, size());
        _backgroundLabel.setLayer(1);

        _riverLabel.setGeometry(0, size());
        _riverLabel.setLayer(2);

        _cloudLabel.setGeometry(0, size());
        _cloudLabel.setLayer(2);

        _treeLabel.setGeometry(0, size());
        _treeLabel.setLayer(3);

		_mainMenu.setGeometry(10, size() / spk::Vector2(2.5f, 1.5f));
        _mainMenu.setLayer(4);
    }

    void _onRender() override
    {

    }

public:
    MainMenuPanel(spk::Widget* p_parent) :
        MainMenuPanel("Unnamed MainMenuPanel", p_parent)
    {

    }

    MainMenuPanel(const std::string& p_name, spk::Widget* p_parent) :
        spk::Panel(p_name, p_parent),
        _backgroundLabel("BackgroundImageLabel", this),
        _riverLabel("RiverImageLabel", this),
        _cloudLabel("CloudImageLabel", this),
        _treeLabel("TreeImageLabel", this),
		_mainMenu("MainMenu", this)
    {
        _backgroundLabel.label().setTexture(TextureManager::instance()->spriteSheet("MainMenuBackground"));
        _backgroundLabel.label().setTextureGeometry(spk::Vector2(0, 0), spk::Vector2(1, 1));
        _backgroundLabel.activate();

        _riverLabel.label().setTexture(TextureManager::instance()->spriteSheet("MainMenuRiverAnimation"));
        _riverLabel.label().setTextureGeometry(spk::Vector2(0, 0), spk::Vector2(1, 1));
        _riverLabel.activate();
        
        _cloudLabel.label().setTexture(TextureManager::instance()->spriteSheet("MainMenuSkyAnimation"));
        _cloudLabel.label().setTextureGeometry(spk::Vector2(0, 0), spk::Vector2(1, 1));
        _cloudLabel.activate();
        
        _treeLabel.label().setTexture(TextureManager::instance()->spriteSheet("MainMenuTreeAnimation"));
        _treeLabel.label().setTextureGeometry(spk::Vector2(0, 0), spk::Vector2(1, 1));
        _treeLabel.activate();

		_mainMenu.activate();
    }
};

class MainWidget : public spk::Widget
{
private:
    TextureManager::Instanciator _textureManagerInstanciator;
    MainMenuPanel _mainMenuPanel;

    void _onUpdate() override
    {
        
    }

    void _onGeometryChange() override
    {
        _mainMenuPanel.setGeometry(0, size());
    }

    void _onRender() override
    {

    }

public:
    MainWidget(spk::Widget* p_parent) :
        MainWidget("Unnamed MainWidget", p_parent)
    {

    }

    MainWidget(const std::string& p_name, spk::Widget* p_parent) :
        spk::Widget(p_name, p_parent),
	    _textureManagerInstanciator(),
        _mainMenuPanel("MainMenuPanel", this)
    {
        _mainMenuPanel.activate();
    }
};

int main()
{
    spk::Application app = spk::Application("Erelia", spk::Vector2UInt(800, 800), spk::Application::Mode::Multithread);

    MainWidget mainWidget(nullptr);
	mainWidget.setGeometry(0, app.size());
	mainWidget.activate();

    return (app.run());
}