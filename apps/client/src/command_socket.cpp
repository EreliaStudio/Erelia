#include "erelia/client/command_socket.hpp"
#include <diagnostics/logger.hpp>
#include <WinSock2.h>
#include <WS2tcpip.h>
#include <array>
#include <chrono>
#include <functional>
#include <stdexcept>
#include <utility>

namespace erelia::client
{
    CommandSocket::CommandSocket(std::uint16_t port)
    {
        if (port) _thread = std::jthread([this, port](std::stop_token stop) { run(stop, port); });
    }
    CommandSocket::~CommandSocket()
    {
        _thread.request_stop();
        if (_thread.joinable()) _thread.join();
    }
    void CommandSocket::dispatch(const std::function<bool(const std::string &)> &execute)
    {
        std::vector<std::shared_ptr<Pending>> ready;
        {
            std::scoped_lock lock(_mutex);
            ready.swap(_pending);
        }
        for (const auto &request : ready)
        {
            try { request->completion.set_value(execute(request->text)); }
            catch (...) { request->completion.set_exception(std::current_exception()); }
        }
    }
    void CommandSocket::run(std::stop_token stop, std::uint16_t port)
    {
        WSADATA wsa{};
        if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) return;
        SOCKET listener = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
        if (listener == INVALID_SOCKET) { WSACleanup(); return; }
        sockaddr_in address{};
        address.sin_family = AF_INET;
        address.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
        address.sin_port = htons(port);
        if (bind(listener, reinterpret_cast<sockaddr *>(&address), sizeof(address)) != 0
            || listen(listener, 4) != 0)
        {
            SPK_LOG(Error) << "COMMAND_SOCKET_ERROR bind" << std::endl;
            closesocket(listener);
            WSACleanup();
            return;
        }
        SPK_LOG(Info) << "COMMAND_SOCKET_READY port=" << port << std::endl;
        while (!stop.stop_requested())
        {
            fd_set reading;
            FD_ZERO(&reading);
            FD_SET(listener, &reading);
            timeval timeout{0, 100000};
            if (select(0, &reading, nullptr, nullptr, &timeout) <= 0) continue;
            SOCKET peer = accept(listener, nullptr, nullptr);
            if (peer == INVALID_SOCKET) continue;
            DWORD readTimeout = 1500;
            setsockopt(peer, SOL_SOCKET, SO_RCVTIMEO, reinterpret_cast<const char *>(&readTimeout), sizeof(readTimeout));
            std::string input;
            char c = 0;
            while (input.size() < 4096 && recv(peer, &c, 1, 0) == 1)
            {
                if (c == '\n') break;
                if (c != '\r') input += c;
            }
            bool success = false;
            if (!input.empty() && input.size() < 4096)
            {
                auto item = std::make_shared<Pending>();
                item->text = std::move(input);
                auto future = item->completion.get_future();
                {
                    std::scoped_lock lock(_mutex);
                    _pending.push_back(std::move(item));
                }
                if (future.wait_for(std::chrono::seconds(3)) == std::future_status::ready)
                {
                    try { success = future.get(); } catch (...) {}
                }
            }
            const char *result = success ? "OK\n" : "ERROR\n";
            send(peer, result, static_cast<int>(strlen(result)), 0);
            closesocket(peer);
        }
        closesocket(listener);
        WSACleanup();
    }
}
