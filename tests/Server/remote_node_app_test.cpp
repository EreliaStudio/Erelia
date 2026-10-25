#include <gtest/gtest.h>

#include "erelia/remote_node/application.hpp"

#include <network/server.hpp>

#include <atomic>

TEST(RemoteNodeApplication, ReleasesListenerWhenStopped)
{
    spk::Server probe;
    probe.start(0);
    const auto port = probe.port();
    probe.stop();

    const erelia::core::RemoteNodeConfiguration configuration{
        .id = "server-test",
        .listenPort = port
    };
    const std::atomic_bool running{false};
    ASSERT_EQ(erelia::remote_node::run(configuration, running), 0);

    EXPECT_NO_THROW(probe.start(port));
    probe.stop();
}
