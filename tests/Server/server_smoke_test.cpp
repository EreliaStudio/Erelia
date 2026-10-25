#include <gtest/gtest.h>

#include <type_traits>
#include "erelia/server/processes.hpp"

// Bootstrap compile/link test; formal M0 requirement linkage is pending.
TEST(ServerFoundationSmoke, HasBothServerEntryPoints)
{
    static_assert(std::is_same_v<decltype(&erelia::server::runRouterPlaceholder),
        int (*)(const erelia::core::ServerRouterConfiguration &, const std::atomic_bool &)>);
    static_assert(std::is_same_v<decltype(&erelia::server::runRemoteNodePlaceholder),
        int (*)(const erelia::core::RemoteNodeConfiguration &, const std::atomic_bool &)>);
}
