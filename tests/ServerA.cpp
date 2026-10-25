#include <gtest/gtest.h>
#include <erelia/shared/ping.hpp>
#include "erelia/server_a/server.hpp"

TEST(ServerATest, RespondsOnlyToOwnPingAndPreservesRequest)
{
    auto request = erelia::ping::request('A', 1234, 987654);
    auto result = erelia::server_a::respond(request, true);
    ASSERT_TRUE(result.has_value());
    EXPECT_EQ(result->type(), erelia::ping::ResponseA);
    EXPECT_EQ(result->requestID(), 1234u);
    EXPECT_EQ(erelia::ping::read(*result).emittedMicroseconds, 987654);
    EXPECT_FALSE(erelia::server_a::respond(erelia::ping::request('B', 10, 20), true).has_value());
    EXPECT_FALSE(erelia::server_a::respond(request, false).has_value());
}
