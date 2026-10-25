#pragma once

#include "erelia/core/configuration.hpp"

#include <network/remote_node.hpp>

#include <atomic>
#include <functional>

namespace erelia::server
{
    using RemoteNodeRequest = spk::RemoteNode::Endpoint::Request;
    using RemoteNodeRequestHandler =
        std::function<void(spk::RemoteNode::Endpoint &, const RemoteNodeRequest &)>;

    int serveRemoteNode(
        const core::RemoteNodeConfiguration &configuration,
        const std::atomic_bool &running,
        const RemoteNodeRequestHandler &handleRequest);
}
