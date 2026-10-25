#include <gtest/gtest.h>

#include "erelia/router/application.hpp"

#include <network/server.hpp>

#include <atomic>

TEST(RouterApplication, ReleasesListenerWhenStopped)
{
    spk::Server probe;
    probe.start(0);
    const auto port = probe.port();
    probe.stop();

    const erelia::core::ServerRouterConfiguration configuration{
        .id = "router-test",
        .listenPort = port
    };
    const std::atomic_bool running{false};
    ASSERT_EQ(erelia::router::run(configuration, running), 0);

    EXPECT_NO_THROW(probe.start(port));
    probe.stop();
}
