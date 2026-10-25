#include "erelia/core/configuration.hpp"
#include "erelia/router/application.hpp"

#include <atomic>
#include <csignal>
#include <exception>
#include <iostream>

namespace
{
    std::atomic_bool running{true};

    void requestStop(int)
    {
        running.store(false);
    }
}

int main(int argc, char **argv)
{
    try
    {
        const auto args = erelia::core::parseConfigurationArguments(argc, argv);
        const auto configuration = erelia::core::loadServerRouterConfiguration(args.file);
        if (args.validateOnly)
        {
            return 0;
        }

        std::signal(SIGINT, requestStop);
        std::signal(SIGTERM, requestStop);
        return erelia::router::run(configuration, running);
    }
    catch (const std::exception &error)
    {
        std::cerr << "EreliaServerRouter: " << error.what() << '\n';
        return 1;
    }
}
