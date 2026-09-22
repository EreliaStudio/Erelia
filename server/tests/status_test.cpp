#include "erelia/server/status.hpp"

#include <gtest/gtest.h>

TEST(ServerInfrastructure, ReturnsSuccess)
{
	EXPECT_EQ(erelia::server::status(), 0);
}
