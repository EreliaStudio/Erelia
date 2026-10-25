#include "erelia/node_app/application.hpp"

#include "erelia/server/remote_node_host.hpp"

namespace erelia::node_app
{
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
