#pragma once

#include "erelia/client/command/connect_command.hpp"

#include <functional>
#include <string>

#include <system/command_parser.hpp>
#include <ui/widget/text_edit.hpp>

class ConsoleEntry final : public spk::TextEdit
{
public:
	using LocalOutputCallback = std::function<void(std::string)>;

private:
	spk::CommandParser _commandParser;
	LocalOutputCallback _localOutput;
	ConnectCommand &_connectCommand;

	void _emitLocal(std::string message);
	void _emitFailure(const spk::CommandParser::Result &result);

public:
	ConsoleEntry(
		std::string name,
		LocalOutputCallback localOutput,
		spk::Widget *parent = nullptr);

	void submit(std::string input);
	[[nodiscard]] ConnectCommand::RequestContract subscribeToConnectRequest(ConnectCommand::RequestCallback callback);
	[[nodiscard]] spk::CommandParser &commandParser() noexcept;
	[[nodiscard]] const spk::CommandParser &commandParser() const noexcept;
};
