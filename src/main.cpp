#include <sparkle.hpp>

#include "context.hpp"

class GameHUD : public spk::Screen
{
private:
	spk::GameEngineWidget _gameEngineWidget;

	Context::Instanciator _contextInstanciator;

	void _onGeometryChange()
	{
		_gameEngineWidget.setGeometry({}, geometry().size);
	}

public:
	GameHUD(const std::wstring &p_name, spk::SafePointer<spk::Widget> p_parent) :
		spk::Screen(p_name, p_parent),
		_gameEngineWidget(p_name + L"/GameEngineWidget", this)
	{
		_gameEngineWidget.setGameEngine(&(Context::instance()->engine));
		_gameEngineWidget.activate();
	}
};

int main()
{
	spk::GraphicalApplication app;

	spk::SafePointer<spk::Window> win = app.createWindow(L"Erelia", {{0, 0}, {800, 600}});

	GameHUD testWidget = GameHUD(L"GameHUD", win->widget());
	testWidget.setGeometry({0, 0}, win->geometry().size);
	testWidget.activate();

	return (app.run());
}