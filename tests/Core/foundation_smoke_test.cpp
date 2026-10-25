#include <gtest/gtest.h>

#include "erelia/core/foundation.hpp"

// Bootstrap smoke test; formal M0 requirement linkage awaits owner-approved ERD IDs.
TEST(CoreFoundationSmoke, IdentifiesModule)
{
    EXPECT_EQ(erelia::core::componentName(), "Core");
}
