#pragma once
#include <cstdint>
#include <future>
#include <mutex>
#include <string>
#include <thread>
#include <vector>
#include <memory>

namespace erelia::client
{
    class CommandSocket
    {
        struct Pending { std::string text; std::promise<bool> completion; };
        std::mutex _mutex;
        std::vector<std::shared_ptr<Pending>> _pending;
        std::jthread _thread;
        void run(std::stop_token stop, std::uint16_t port);
    public:
        explicit CommandSocket(std::uint16_t port);
        ~CommandSocket();
        void dispatch(const std::function<bool(const std::string &)> &execute);
    };
}
