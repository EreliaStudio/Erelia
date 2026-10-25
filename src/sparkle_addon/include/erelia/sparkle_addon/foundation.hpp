#pragma once

#include <string_view>

namespace erelia::sparkle_addon
{
    [[nodiscard]] std::string_view componentName() noexcept;
}
