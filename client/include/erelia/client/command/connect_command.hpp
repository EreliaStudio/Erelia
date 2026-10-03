#pragma once

#include <system/command_parser.hpp>

class ConnectCommand final : public spk::CommandParser::Command
{
public:
	ConnectCommand();

	void execute(const spk::CommandParser::Invocation &invocation) override;
};
