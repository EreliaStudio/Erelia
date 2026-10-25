#include "erelia/node_app/application.hpp"

#include <csignal>

#include "erelia/server/remote_node_host.hpp"

namespace
{
    std::atomic_bool running{true};

    void requestStop(int)
    {
        running.store(false);
    }
}

namespace erelia::node_app
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
        return server::serveRemoteNode(
            configuration,
            running,
            [](spk::RemoteNode::Endpoint &endpoint, const server::RemoteNodeRequest &request)
            {
                // Implement the specialized node's protocol-specific request handling here.
                (void)endpoint;
                (void)request;
            });
    }
}
