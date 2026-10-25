#include <gtest/gtest.h>

#include "erelia/server/remote_node_host.hpp"

#include <type_traits>

// Verify the shared server library exposes the reusable node host.
TEST(ServerFoundationSmoke, ProvidesRemoteNodeHost)
{
    static_assert(std::is_same_v<decltype(&erelia::server::serveRemoteNode),
        int (*)(const erelia::core::RemoteNodeConfiguration &,
                const std::atomic_bool &,
                const erelia::server::RemoteNodeRequestHandler &)>);
}
