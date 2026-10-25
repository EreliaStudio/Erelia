#pragma once
#include "erelia/client/ping_controller.hpp"
#include <system/command_parser.hpp>
#include <string>

namespace erelia::client
{
    class Commands
    {
        spk::CommandParser _parser;
        PingController &_controller;
    public:
        explicit Commands(PingController &controller);
        [[nodiscard]] bool execute(const std::string &text, const std::string &source);
    };
}
