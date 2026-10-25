#include "erelia/server/processes.hpp"

#include <iostream>

namespace erelia::server
{
    int runRouterPlaceholder(const erelia::core::ServerRouterConfiguration &configuration)
    {
        std::cout << "Erelia Server Router [" << configuration.id << "], port "
                  << configuration.listenPort << ": M1 placeholder (network not started)\n";
        for (const auto &node : configuration.remoteNodes)
        {
            std::cout << "  configured remote: " << node.id << " ("
                      << node.endpoint.host << ':' << node.endpoint.port << ")\n";
        }
        return 0;
    }

    int runRemoteNodePlaceholder(const erelia::core::RemoteNodeConfiguration &configuration)
    {
        std::cout << "Erelia RemoteNode [" << configuration.id << "], port "
                  << configuration.listenPort << ": M1 placeholder (network not started)\n";
        return 0;
    }
}
