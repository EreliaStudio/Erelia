#pragma once

#include <string>

#include <system/command_parser.hpp>
#include <ui/widget/text_edit.hpp>

class ConsoleEntry final : public spk::TextEdit
{
private:
	spk::CommandParser _commandParser;

	void _emitFailure(const spk::CommandParser::Result &result);

public:
	ConsoleEntry(
		std::string name,
		spk::Widget *parent = nullptr);

	void submit(std::string input);
	[[nodiscard]] spk::CommandParser &commandParser() noexcept;
	[[nodiscard]] const spk::CommandParser &commandParser() const noexcept;
};
