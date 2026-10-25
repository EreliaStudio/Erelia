#include "erelia/client/application.hpp"

#include "core/application.hpp"

namespace erelia::client
{
    int runApplication()
    {
        spk::Application application;
        spk::Window::Configuration configuration{
            .title = "Erelia (M1 placeholder)",
            .area = {.anchor = {80, 80}, .size = {960, 600}}
        };
        static_cast<void>(application.createWindow("main", configuration));
        return application.run();
    }
}
