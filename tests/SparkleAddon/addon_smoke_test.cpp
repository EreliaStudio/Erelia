#include <gtest/gtest.h>

#include "erelia/sparkle_addon/foundation.hpp"

// Bootstrap smoke test; formal M0 requirement linkage awaits owner-approved ERD IDs.
TEST(SparkleAddonFoundationSmoke, IdentifiesModule)
{
    EXPECT_EQ(erelia::sparkle_addon::componentName(), "SparkleAddon");
}
