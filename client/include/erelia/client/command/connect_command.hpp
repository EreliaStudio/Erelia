#pragma once

#include "erelia/client/event_center.hpp"

#include <system/command_parser.hpp>

class ConnectCommand final : public spk::CommandParser::Command
{
public:
	using Request = Client::ConnectionRequest;

	ConnectCommand();

	void execute(const spk::CommandParser::Invocation &invocation) override;
};
