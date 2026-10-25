#pragma once
#include <erelia/shared/ping.hpp>
#include <array>
#include <chrono>
#include <cstdint>
#include <functional>
#include <network/message.hpp>

namespace erelia::client
{
    enum class PingStatus { Idle, Waiting, Received, Failed };
    struct PingRecord
    {
        PingStatus status = PingStatus::Idle;
        spk::Message::RequestID requestID = 0;
        std::int64_t emitted = 0;
        std::int64_t latencyMicroseconds = 0;
        std::chrono::steady_clock::time_point deadline{};
    };

    class PingController
    {
    public:
        using Sender = std::function<void(const spk::Message &)>;
    private:
        std::array<PingRecord, 2> _records;
        std::uint64_t _nextRequestID = 1;
        std::chrono::milliseconds _timeout;
        Sender _send;
        [[nodiscard]] static std::size_t index(char server);
    public:
        PingController(std::chrono::milliseconds timeout, Sender send);
        [[nodiscard]] const PingRecord &state(char server) const;
        [[nodiscard]] bool ping(char server);
        [[nodiscard]] bool receive(const spk::Message &message);
        void poll();
    };
}
