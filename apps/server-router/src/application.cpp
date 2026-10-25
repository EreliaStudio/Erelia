#include "erelia/router/application.hpp"

#include <network/node_router.hpp>

#include <chrono>
#include <iostream>
#include <thread>

namespace erelia::router
{
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
