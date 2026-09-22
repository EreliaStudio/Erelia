#include "erelia/client/status.hpp"

#include <gtest/gtest.h>

TEST(ClientInfrastructure, ReturnsSuccess)
{
	EXPECT_EQ(erelia::client::status(), 0);
}
