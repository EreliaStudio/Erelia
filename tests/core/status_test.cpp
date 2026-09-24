#include "erelia/core/status.hpp"

#include <gtest/gtest.h>

TEST(CoreInfrastructure, ReturnsSuccess)
{
	EXPECT_EQ(erelia::core::status(), 0);
}
