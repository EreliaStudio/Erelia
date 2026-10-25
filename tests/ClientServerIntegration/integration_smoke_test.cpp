#include <gtest/gtest.h>

#ifdef ERELIA_WITH_CLIENT
#include "erelia/client/application.hpp"
#endif
#include "erelia/server/processes.hpp"

#include <network/server.hpp>

#include <atomic>
#include <cstdint>
#include <type_traits>

// The compile-only check verifies module linkage, not communication between applications.
TEST(ClientServerIntegrationScaffold, BothSidesLink)
{
#ifdef ERELIA_WITH_CLIENT
    static_assert(std::is_same_v<decltype(&erelia::client::runApplication), int (*)()>);
#endif
    static_assert(std::is_same_v<decltype(&erelia::server::runRouterPlaceholder),
        int (*)(const erelia::core::ServerRouterConfiguration &, const std::atomic_bool &)>);
}

// Verify real listener startup and shutdown, including reuse of the released socket.
// This is a lifecycle smoke test; Router-to-node routing is not implemented in M1.
TEST(ClientServerIntegrationScaffold, RouterAndRemoteNodeReleaseListeningPorts)
{
    spk::Server probe;
    probe.start(0);
    const std::uint16_t port = probe.port();
    ASSERT_NE(port, 0);
    probe.stop();

    const std::atomic_bool running{false};
    const erelia::core::ServerRouterConfiguration router{
        .id = "smoke-router",
        .listenPort = port
    };
    const erelia::core::RemoteNodeConfiguration remoteNode{
        .id = "smoke-node",
        .listenPort = port
    };

    EXPECT_EQ(erelia::server::runRouterPlaceholder(router, running), 0);
    EXPECT_EQ(erelia::server::runRemoteNodePlaceholder(remoteNode, running), 0);

    ASSERT_NO_THROW(probe.start(port));
    EXPECT_TRUE(probe.isRunning());
    probe.stop();
}
