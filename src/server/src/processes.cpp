#include "erelia/server/processes.hpp"

#include "erelia/server/remote_node_host.hpp"

#include <network/node_router.hpp>

#include <chrono>
#include <iostream>
#include <thread>

namespace erelia::server
{
    int runRouterPlaceholder(
        const erelia::core::ServerRouterConfiguration &configuration,
        const std::atomic_bool &running)
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

    int runRemoteNodePlaceholder(
        const erelia::core::RemoteNodeConfiguration &configuration,
        const std::atomic_bool &running)
    {
        std::cout << "Erelia RemoteNode [" << configuration.id << "] listening on port "
                  << configuration.listenPort << '\n';
        return serveRemoteNode(
            configuration,
            running,
            [](spk::RemoteNode::Endpoint &, const RemoteNodeRequest &)
            {
                // The base node intentionally provides no domain messages.
            });
    }
}
