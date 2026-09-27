#pragma once

#include <system/command_parser.hpp>

class ConsoleEntry;

class ConnectCommand final : public spk::CommandParser::Command
{
private:
	ConsoleEntry &_owner;

public:
	explicit ConnectCommand(ConsoleEntry &owner);

	void execute(const spk::CommandParser::Invocation &invocation) override;
};
