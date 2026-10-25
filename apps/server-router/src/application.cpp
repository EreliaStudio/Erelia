#include "erelia/router/application.hpp"

#include <csignal>

#include <network/node_router.hpp>

#include <chrono>
#include <iostream>
#include <thread>

namespace
{
    std::atomic_bool running{true};

    void requestStop(int)
    {
        running.store(false);
    }
}

namespace erelia::router
{
    int runApplication(int argc, char **argv)
    {
        const auto args = core::parseConfigurationArguments(argc, argv);
        const auto configuration = core::loadServerRouterConfiguration(args.file);
        if (args.validateOnly)
        {
            return 0;
        }

        running.store(true);
        std::signal(SIGINT, requestStop);
        std::signal(SIGTERM, requestStop);
        return run(configuration, running);
    }

    int run(const core::ServerRouterConfiguration &configuration, const std::atomic_bool &running)
    {
        spk::NodeRouter router;
        router.start(configuration.listenPort);
        std::cout << "Erelia Server Router [" << configuration.id << "] listening on port "
                  << configuration.listenPort << '\n';

        try
        {
            while (running.load())
            {
                router.dispatch();
                std::this_thread::sleep_for(std::chrono::milliseconds(10));
            }
        }
        catch (...)
        {
            router.stop();
            throw;
        }
        router.stop();
        return 0;
    }
}
