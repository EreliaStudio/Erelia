#include "erelia/core/configuration.hpp"
#include "erelia/server/remote_node_host.hpp"

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
        const auto configuration = erelia::core::loadRemoteNodeConfiguration(args.file);
        if (args.validateOnly)
        {
            return 0;
        }

        std::signal(SIGINT, requestStop);
        std::signal(SIGTERM, requestStop);

        return erelia::server::serveRemoteNode(
            configuration,
            running,
            [](spk::RemoteNode::Endpoint &endpoint, const erelia::server::RemoteNodeRequest &request)
            {
                // Implement this node's protocol-specific request handling here.
                // The generic host owns the listening socket and dispatch loop.
                (void)endpoint;
                (void)request;
            });
    }
    catch (const std::exception &error)
    {
        std::cerr << "EreliaExampleNode: " << error.what() << '\n';
        return 1;
    }
}
