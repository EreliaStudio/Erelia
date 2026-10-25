#include <gtest/gtest.h>

#include "erelia/node_app/application.hpp"

#include <network/server.hpp>

#include <atomic>

TEST(ExampleNodeApplication, ReleasesListenerWhenStopped)
{
    spk::Server probe;
    probe.start(0);
    const auto port = probe.port();
    probe.stop();

    const erelia::core::RemoteNodeConfiguration configuration{
        .id = "example-node-test",
        .listenPort = port
    };
    const std::atomic_bool running{false};
    ASSERT_EQ(erelia::node_app::run(configuration, running), 0);

    EXPECT_NO_THROW(probe.start(port));
    probe.stop();
}
