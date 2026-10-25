#include <gtest/gtest.h>

#include <type_traits>
#include "erelia/client/application.hpp"
#include "erelia/server/processes.hpp"

// This validates suite discovery and linking, NOT application communication.
TEST(ClientServerIntegrationScaffold, BothSidesLink)
{
    static_assert(std::is_same_v<decltype(&erelia::client::runApplication), int (*)()>);
    static_assert(std::is_same_v<decltype(&erelia::server::runRouterPlaceholder),
        int (*)(const erelia::core::ServerRouterConfiguration &, const std::atomic_bool &)>);
}
