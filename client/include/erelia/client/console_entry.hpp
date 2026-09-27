#pragma once

#include <functional>
#include <string>

#include <system/command_parser.hpp>
#include <ui/widget/text_edit.hpp>

class ConnectionManager;

class ConsoleEntry final : public spk::TextEdit
{
public:
	using LocalOutputCallback = std::function<void(std::string)>;

private:
	ConnectionManager &_connectionManager;
	spk::CommandParser _commandParser;
	LocalOutputCallback _localOutput;

	void _emitLocal(std::string message);
	void _emitFailure(const spk::CommandParser::Result &result);
	void _registerCommands();

public:
	ConsoleEntry(
		std::string name,
		ConnectionManager &connectionManager,
		LocalOutputCallback localOutput,
		spk::Widget *parent = nullptr);

	void submit(std::string input);
	[[nodiscard]] spk::CommandParser &commandParser() noexcept;
	[[nodiscard]] const spk::CommandParser &commandParser() const noexcept;
};
