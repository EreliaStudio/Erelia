#include "erelia/client/console_entry.hpp"

#include "erelia/client/service.hpp"

#include <diagnostics/logger.hpp>
#include <system/translator.hpp>

#include <limits>
#include <utility>

ConsoleEntry::ConsoleEntry(
	std::string name,
	spk::Widget *parent) :
	spk::TextEdit(std::move(name), parent)
{
	setPlaceholder(
		Service::translator()->translate(
			"client.console.placeholder"));
	setMaximalSize({std::numeric_limits<float>::max(), maximalSize().y});
}

void ConsoleEntry::_emitFailure(const spk::CommandParser::Result &result)
{
	std::string message;
	switch (result.status)
	{
	case spk::CommandParser::Status::UnknownCommand:
		message = Service::translator()->translate(
			"client.console.command.unknown",
			result.command);
		break;
	case spk::CommandParser::Status::InvalidFormat:
		message = Service::translator()->translate(
			"client.console.command.invalid_format");
		break;
	case spk::CommandParser::Status::UnknownParameter:
		message = Service::translator()->translate(
			"client.console.command.parameter_unknown",
			result.parameter);
		break;
	case spk::CommandParser::Status::DuplicateParameter:
		message = Service::translator()->translate(
			"client.console.command.parameter_duplicate",
			result.parameter);
		break;
	case spk::CommandParser::Status::MissingParameter:
		message = Service::translator()->translate(
			"client.console.command.parameter_missing",
			result.parameter);
		break;
	case spk::CommandParser::Status::MissingValue:
		message = Service::translator()->translate(
			"client.console.command.value_missing",
			result.parameter,
			result.expectedValueCount);
		break;
	case spk::CommandParser::Status::TooManyValues:
		message = Service::translator()->translate(
			"client.console.command.too_many_values",
			result.parameter,
			result.expectedValueCount,
			result.actualValueCount);
		break;
	case spk::CommandParser::Status::TooManyParameters:
		message = Service::translator()->translate(
			"client.console.command.too_many_parameters");
		break;
	case spk::CommandParser::Status::Accepted:
	case spk::CommandParser::Status::HelpRequested:
		return;
	}

	SPK_LOG(UserValueB) << message << std::endl;

	if (result.command.empty() == false)
	{
		const std::string usage = _commandParser.help(result.command);
		if (usage.empty() == false)
		{
			SPK_LOG(UserValueB) << usage << std::endl;
		}
	}
}

void ConsoleEntry::submit(std::string input)
{
	if (input.empty() == true)
	{
		return;
	}
	if (input == "/help")
	{
		SPK_LOG(UserValueB) << _commandParser.help() << std::endl;
		return;
	}
	if (input.front() != '/')
	{
		SPK_LOG(UserValueA) << input << std::endl;
		return;
	}

	const spk::CommandParser::Result result = _commandParser.execute(input);
	if (result.status == spk::CommandParser::Status::HelpRequested)
	{
		SPK_LOG(UserValueB)
			<< _commandParser.help(result.command)
			<< std::endl;
		return;
	}
	if (result.status != spk::CommandParser::Status::Accepted)
	{
		_emitFailure(result);
	}
}

spk::CommandParser &ConsoleEntry::commandParser() noexcept
{
	return _commandParser;
}

const spk::CommandParser &ConsoleEntry::commandParser() const noexcept
{
	return _commandParser;
}
