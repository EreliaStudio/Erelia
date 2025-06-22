#include <sparkle.hpp>

#include "main_application.hpp"

int main()
{
	spk::GraphicalApplication app;

	spk::SafePointer<spk::Window> win = app.createWindow(L"Erelia", {{0, 0}, {800, 600}});

	MainApplication mainMenu = MainApplication(L"MainApplication", win->widget());
	mainMenu.setOnQuitApplication([&](){app.quit(0);});
	mainMenu.setGeometry({0, win->geometry().size});
	mainMenu.activate();

	return (app.run());
}