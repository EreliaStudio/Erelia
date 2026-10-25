#include "erelia/client/ping_controller.hpp"
#include <diagnostics/logger.hpp>
#include <stdexcept>
#include <utility>

namespace erelia::client
{
    std::size_t PingController::index(char server)
    {
        if (server == 'A') return 0;
        if (server == 'B') return 1;
        throw std::invalid_argument("Unknown ping target");
    }
    PingController::PingController(std::chrono::milliseconds timeout, Sender send) :
        _timeout(timeout), _send(std::move(send))
    {
        if (_timeout.count() <= 0 || !_send)
            throw std::invalid_argument("Ping requires a positive timeout and sender");
    }
    const PingRecord &PingController::state(char server) const { return _records[index(server)]; }
    bool PingController::ping(char server)
    {
        auto &record = _records[index(server)];
        if (record.status == PingStatus::Waiting) return false;
        auto id = _nextRequestID++;
        auto emitted = erelia::ping::timestamp();
        auto packet = erelia::ping::request(server, id, emitted);
        _send(packet);
        record.status = PingStatus::Waiting;
        record.requestID = id;
        record.emitted = emitted;
        record.deadline = std::chrono::steady_clock::now() + _timeout;
        SPK_LOG(Info) << "PING_SENT target=" << server << " id=" << id << " emitted=" << emitted << std::endl;
        return true;
    }
    bool PingController::receive(const spk::Message &message)
    {
        const char server = message.type() == ping::ResponseA ? 'A'
            : message.type() == ping::ResponseB ? 'B' : '\0';
        if (!server) return false;
        auto &record = _records[index(server)];
        if (record.status != PingStatus::Waiting || record.requestID != message.requestID()
            || std::chrono::steady_clock::now() >= record.deadline)
            return false;
        const auto payload = ping::read(message);
        if (payload.emittedMicroseconds != record.emitted) return false;
        record.latencyMicroseconds = ping::timestamp() - record.emitted;
        record.status = PingStatus::Received;
        SPK_LOG(Info) << "PING_RESULT target=" << server << " status=Received id="
                      << record.requestID << " latency_us=" << record.latencyMicroseconds << std::endl;
        return true;
    }
    void PingController::poll()
    {
        for (std::size_t i = 0; i < _records.size(); ++i)
        {
            auto &record = _records[i];
            if (record.status == PingStatus::Waiting && std::chrono::steady_clock::now() >= record.deadline)
            {
                record.status = PingStatus::Failed;
                SPK_LOG(Warning) << "PING_RESULT target=" << (i == 0 ? 'A' : 'B')
                                 << " status=Failed id=" << record.requestID << std::endl;
            }
        }
    }
}
