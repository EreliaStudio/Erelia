#pragma once
#include <network/message.hpp>
#include <cstdint>
#include <chrono>

namespace erelia::ping
{
    inline constexpr spk::Message::Type RequestA = 7101;
    inline constexpr spk::Message::Type ResponseA = 7102;
    inline constexpr spk::Message::Type RequestB = 7103;
    inline constexpr spk::Message::Type ResponseB = 7104;

    struct Payload
    {
        std::int64_t emittedMicroseconds = 0;
    };

    [[nodiscard]] std::int64_t timestamp();
    [[nodiscard]] spk::Message request(char target, spk::Message::RequestID id, std::int64_t emitted);
    [[nodiscard]] spk::Message response(char target, const spk::Message &received);
    [[nodiscard]] Payload read(const spk::Message &message);
    [[nodiscard]] spk::Message::Type requestType(char target);
    [[nodiscard]] spk::Message::Type responseType(char target);
}
