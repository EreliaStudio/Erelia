#include "erelia/client/application.hpp"

#include "core/application.hpp"

namespace erelia::client
{
    int runApplication(const erelia::core::ClientConfiguration &settings)
    {
        spk::Application application;
        spk::Window::Configuration configuration{
            .title = settings.window.title,
            .area = {.anchor = {80, 80}, .size = {settings.window.width, settings.window.height}}
        };
        static_cast<void>(application.createWindow("main", configuration));
        return application.run();
    }
}
