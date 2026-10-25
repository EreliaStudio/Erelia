#include "erelia/remote_node/application.hpp"

#include <csignal>

#include "erelia/server/remote_node_host.hpp"

#include <iostream>

namespace
{
    std::atomic_bool running{true};

    void requestStop(int)
    {
        running.store(false);
    }
}

namespace erelia::remote_node
{
    int runApplication(int argc, char **argv)
    {
        const auto args = core::parseConfigurationArguments(argc, argv);
        const auto configuration = core::loadRemoteNodeConfiguration(args.file);
        if (args.validateOnly)
        {
            return 0;
        }

        running.store(true);
        std::signal(SIGINT, requestStop);
        std::signal(SIGTERM, requestStop);
        return run(configuration, running);
    }

    int run(const core::RemoteNodeConfiguration &configuration, const std::atomic_bool &running)
    {
        std::cout << "Erelia RemoteNode [" << configuration.id << "] listening on port "
                  << configuration.listenPort << '\n';
        return server::serveRemoteNode(
            configuration,
            running,
            [](spk::RemoteNode::Endpoint &, const server::RemoteNodeRequest &)
            {
                // Generic RemoteNode has no application-specific protocol yet.
            });
    }
}
