#pragma once

#include "erelia/core/configuration.hpp"

#include <atomic>

namespace erelia::router
{
    int run(const core::ServerRouterConfiguration &configuration, const std::atomic_bool &running);
}
