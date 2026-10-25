#include "erelia/server/remote_node_host.hpp"

#include <chrono>
#include <stdexcept>
#include <thread>
#include <vector>

namespace erelia::server
{
    int serveRemoteNode(
        const core::RemoteNodeConfiguration &configuration,
        const std::atomic_bool &running,
        const RemoteNodeRequestHandler &handleRequest)
    {
        if (!handleRequest)
        {
            throw std::invalid_argument("A RemoteNode must define a request handler");
        }

        spk::RemoteNode::Endpoint endpoint;
        endpoint.start(configuration.listenPort);

        std::vector<RemoteNodeRequest> requests;
        try
        {
            while (running.load())
            {
                endpoint.dispatch();
                for (const RemoteNodeRequest &request : endpoint.requests().drain(requests))
                {
                    handleRequest(endpoint, request);
                }
                std::this_thread::sleep_for(std::chrono::milliseconds(10));
            }
        }
        catch (...)
        {
            endpoint.stop();
            throw;
        }

        endpoint.stop();
        return 0;
    }
}
