#pragma once

#include "erelia/core/configuration.hpp"

#include <atomic>

namespace erelia::remote_node
{
    int run(const core::RemoteNodeConfiguration &configuration, const std::atomic_bool &running);
}
