#include <gtest/gtest.h>

#include "erelia/server/remote_node_host.hpp"

#include <atomic>
#include <stdexcept>

// Bootstrap test: replace with an approved Test ID / requirement reference.
TEST(RemoteNodeHostTest, RequiresARequestHandler)
{
    const erelia::core::RemoteNodeConfiguration configuration{
        .id = "test-node",
        .listenPort = 43031
    };
    const std::atomic_bool running{false};
    EXPECT_THROW(erelia::server::serveRemoteNode(configuration, running, {}), std::invalid_argument);
}
