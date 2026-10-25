#pragma once

#include "erelia/core/configuration.hpp"

#include <atomic>

namespace erelia::server
{
    int runRouterPlaceholder(
        const erelia::core::ServerRouterConfiguration &configuration,
        const std::atomic_bool &running);
    int runRemoteNodePlaceholder(
        const erelia::core::RemoteNodeConfiguration &configuration,
        const std::atomic_bool &running);
}
