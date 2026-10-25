#pragma once
#include <erelia/shared/config.hpp>
#include <network/remote_node.hpp>
#include <optional>

namespace erelia::server_a
{
    [[nodiscard]] std::optional<spk::Message> respond(const spk::Message &request, bool enabled);
    class Server
    {
        spk::RemoteNode::Endpoint _endpoint;
        config::Server _config;
        spk::RemoteNode::Endpoint::MessageDispatcher::Contract _subscription;
    public:
        explicit Server(config::Server configuration);
        void start();
        void dispatch();
        void stop();
    };
}
