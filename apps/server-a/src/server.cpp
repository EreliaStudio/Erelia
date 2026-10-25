#include "erelia/server_a/server.hpp"
#include <erelia/shared/ping.hpp>
#include <diagnostics/logger.hpp>
#include <utility>

namespace erelia::server_a
{
    std::optional<spk::Message> respond(const spk::Message &request, bool enabled)
    {
        if (request.type() != ping::RequestA) return std::nullopt;
        const auto payload = ping::read(request);
        SPK_LOG(Info) << "PING_RECEIVED target=A id=" << request.requestID()
                      << " emitted=" << payload.emittedMicroseconds << std::endl;
        if (!enabled) return std::nullopt;
        auto reply = ping::response('A', request);
        SPK_LOG(Info) << "PING_REPLIED target=A id=" << request.requestID() << std::endl;
        return reply;
    }
    Server::Server(config::Server configuration) : _config(std::move(configuration)) {}
    void Server::start()
    {
        _subscription = _endpoint.messageDispatcher().subscribeTo(ping::RequestA, [this](const auto &request) {
            if (auto reply = respond(request.message, _config.respond))
                _endpoint.reply(request, std::move(*reply));
        });
        _endpoint.start(_config.port);
        SPK_LOG(Info) << "SERVER_A_READY" << std::endl;
    }
    void Server::dispatch()
    {
        _endpoint.dispatch();
        _endpoint.treatMessages();
    }
    void Server::stop() { _endpoint.stop(); }
}
