#pragma once
#include <erelia/shared/config.hpp>
#include <network/node_router.hpp>
#include <network/remote_node.hpp>

namespace erelia::router
{
    class Router
    {
        spk::NodeRouter _router;
        spk::RemoteNode _a;
        spk::RemoteNode _b;
        config::Router _configuration;
    public:
        explicit Router(config::Router configuration);
        void start();
        void dispatch();
        void stop();
    };
}
