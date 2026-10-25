#include <gtest/gtest.h>

#include <type_traits>
#include "erelia/client/application.hpp"

// Bootstrap compile/link test; formal M0 requirement linkage is pending.
TEST(ClientFoundationSmoke, HasApplicationEntryPoint)
{
    static_assert(std::is_same_v<decltype(&erelia::client::runApplication), int (*)()>);
}
