#include "erelia/client/console_entry.hpp"

#include <diagnostics/logger.hpp>

#include <charconv>
#include <limits>
#include <memory>
#include <utility>

class ConsoleEntry::ConnectCommand final : public spk::CommandParser::Command
{
private:
	ConsoleEntry &_owner;

public:
	explicit ConnectCommand(ConsoleEntry &owner) :
		Command(
			"connect",
			"Starts a new dedicated Server connection cycle.",
			{
				{.name = "address", .description = "Dedicated Server address", .optional = true},
				{.name = "port", .description = "Dedicated Server port", .optional = true},
			}),
		_owner(owner)
	{
	}

	void execute(const spk::CommandParser::Invocation &invocation) override
	{
		ConnectRequest request;

		if (invocation.parameters.contains("address") == true)
		{
			request.address = invocation.get("address").front();
		}
		if (invocation.parameters.contains("port") == true)
		{
			const std::string &value = invocation.get("port").front();
			unsigned int port = 0;
			const auto [end, error] = std::from_chars(
				value.data(),
				value.data() + value.size(),
				port);
			if (
				error != std::errc{} ||
				end != value.data() + value.size() ||
				port == 0 ||
				port > std::numeric_limits<std::uint16_t>::max())
			{
				_owner._emitLocal("Invalid port: " + value);
				return;
			}
			request.port = static_cast<std::uint16_t>(port);
		}

		_owner._connectRequestProvider.trigger(request);
	}
};

ConsoleEntry::ConsoleEntry(
	std::string name,
	LocalOutputCallback localOutput,
	spk::Widget *parent) :
	spk::TextEdit(std::move(name), parent),
	_localOutput(std::move(localOutput))
{
	setPlaceholder("Enter text or /help");
	setMaximalSize({std::numeric_limits<float>::max(), maximalSize().y});
	_registerCommands();
}

void ConsoleEntry::_registerCommands()
{
	_commandParser.addCommand(
		std::make_unique<ConnectCommand>(*this));
}

void ConsoleEntry::_emitLocal(std::string message)
{
	if (_localOutput)
	{
		_localOutput(std::move(message));
	}
}

void ConsoleEntry::_emitFailure(const spk::CommandParser::Result &result)
{
	std::string message;
	switch (result.status)
	{
	case spk::CommandParser::Status::UnknownCommand:
		message = "Unknown command: /" + result.command;
		break;
	case spk::CommandParser::Status::InvalidFormat:
		message = "Invalid command format.";
		break;
	case spk::CommandParser::Status::UnknownParameter:
		message = "Unknown parameter: --" + result.parameter;
		break;
	case spk::CommandParser::Status::DuplicateParameter:
		message = "Duplicate parameter: --" + result.parameter;
		break;
	case spk::CommandParser::Status::MissingParameter:
		message = "Missing parameter: --" + result.parameter;
		break;
	case spk::CommandParser::Status::MissingValue:
		message = "Missing value for --" + result.parameter + " (expected " + std::to_string(result.expectedValueCount) + ").";
		break;
	case spk::CommandParser::Status::TooManyValues:
		message = "Too many values for --" + result.parameter + " (expected " + std::to_string(result.expectedValueCount) + ", received " + std::to_string(result.actualValueCount) + ").";
		break;
	case spk::CommandParser::Status::TooManyParameters:
		message = "Too many parameters.";
		break;
	case spk::CommandParser::Status::Accepted:
	case spk::CommandParser::Status::HelpRequested:
		return;
	}

	_emitLocal(std::move(message));
	if (result.command.empty() == false)
	{
		const std::string usage = _commandParser.help(result.command);
		if (usage.empty() == false)
		{
			_emitLocal(usage);
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
		_emitLocal(_commandParser.help());
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
		_emitLocal(_commandParser.help(result.command));
		return;
	}
	if (result.status != spk::CommandParser::Status::Accepted)
	{
		_emitFailure(result);
	}
}

ConsoleEntry::ConnectRequestContract ConsoleEntry::subscribeToConnectRequest(ConnectRequestCallback callback)
{
	return _connectRequestProvider.subscribe(std::move(callback));
}

spk::CommandParser &ConsoleEntry::commandParser() noexcept
{
	return _commandParser;
}

const spk::CommandParser &ConsoleEntry::commandParser() const noexcept
{
	return _commandParser;
}
