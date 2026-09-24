#include "erelia/client/status.hpp"

#include <gtest/gtest.h>

TEST(ClientInfrastructure, ReturnsSuccess)
{
	EXPECT_EQ(status(), 0);
}
