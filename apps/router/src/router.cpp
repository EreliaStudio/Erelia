#include "erelia/router/router.hpp"
#include <erelia/shared/ping.hpp>
#include <diagnostics/logger.hpp>
#include <chrono>
#include <stdexcept>
#include <thread>
#include <utility>

namespace erelia::router
{
    Router::Router(config::Router configuration) : _configuration(std::move(configuration)) {}
    void Router::start()
    {
        _a.connect(_configuration.serverA.host, _configuration.serverA.port);
        _b.connect(_configuration.serverB.host, _configuration.serverB.port);
        auto deadline = std::chrono::steady_clock::now() + std::chrono::seconds(5);
        while ((!_a.isConnected() || !_b.isConnected()) && std::chrono::steady_clock::now() < deadline)
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
        if (!_a.isConnected() || !_b.isConnected()) throw std::runtime_error("Router could not connect to both servers");
        _router.addNode("A", _a);
        _router.addNode("B", _b);
        _router.redirect(ping::RequestA, "A");
        _router.redirect(ping::RequestB, "B");
        _router.start(_configuration.port);
        SPK_LOG(Info) << "ROUTER_READY" << std::endl;
    }
    void Router::dispatch()
    {
        // Incoming unknown message types are reported but never forwarded.
        try { _router.dispatch(); }
        catch (const std::exception &error) { SPK_LOG(Error) << "ROUTER_REJECTED " << error.what() << std::endl; }
        _a.dispatch();
        _b.dispatch();
        _router.dispatch();
    }
    void Router::stop()
    {
        _router.stop();
        _a.disconnect();
        _b.disconnect();
    }
}
