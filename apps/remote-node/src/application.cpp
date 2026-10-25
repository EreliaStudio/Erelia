#include "erelia/remote_node/application.hpp"

#include "erelia/server/remote_node_host.hpp"

#include <iostream>

namespace erelia::remote_node
{
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
