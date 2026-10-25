#pragma once

#include "erelia/core/configuration.hpp"

#include <atomic>

namespace erelia::node_app
{
    int runApplication(int argc, char **argv);
    int run(const core::RemoteNodeConfiguration &configuration, const std::atomic_bool &running);
}
