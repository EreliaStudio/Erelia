#include <gtest/gtest.h>
#include <erelia/shared/ping.hpp>
#include "erelia/server_b/server.hpp"

TEST(ServerBTest, RespondsOnlyToOwnPingAndPreservesRequest)
{
    auto request = erelia::ping::request('B', 1234, 987654);
    auto result = erelia::server_b::respond(request, true);
    ASSERT_TRUE(result.has_value());
    EXPECT_EQ(result->type(), erelia::ping::ResponseB);
    EXPECT_EQ(result->requestID(), 1234u);
    EXPECT_EQ(erelia::ping::read(*result).emittedMicroseconds, 987654);
    EXPECT_FALSE(erelia::server_b::respond(erelia::ping::request('A', 10, 20), true).has_value());
    EXPECT_FALSE(erelia::server_b::respond(request, false).has_value());
}
