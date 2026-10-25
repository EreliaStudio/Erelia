#include "erelia/shared/ping.hpp"
#include <stdexcept>
#include <utility>

namespace erelia::ping
{
    std::int64_t timestamp()
    {
        return std::chrono::duration_cast<std::chrono::microseconds>(
            std::chrono::system_clock::now().time_since_epoch()).count();
    }

    spk::Message::Type requestType(char target)
    {
        if (target == 'A') return RequestA;
        if (target == 'B') return RequestB;
        throw std::invalid_argument("Unknown ping destination");
    }

    spk::Message::Type responseType(char target)
    {
        if (target == 'A') return ResponseA;
        if (target == 'B') return ResponseB;
        throw std::invalid_argument("Unknown ping destination");
    }

    spk::Message request(char target, spk::Message::RequestID id, std::int64_t emitted)
    {
        spk::Message::Writer writer(requestType(target));
        writer.setRequestID(id);
        writer << Payload{emitted};
        return std::move(writer).build();
    }

    Payload read(const spk::Message &message)
    {
        if (message.size() != sizeof(Payload))
            throw std::invalid_argument("Invalid Ping payload length");
        return message.reader().get<Payload>();
    }

    spk::Message response(char target, const spk::Message &received)
    {
        if (received.type() != requestType(target))
            throw std::invalid_argument("Wrong destination for Ping request");
        const Payload payload = read(received);
        spk::Message::Writer writer(responseType(target));
        writer.setRequestID(received.requestID());
        writer << payload;
        return std::move(writer).build();
    }
}
