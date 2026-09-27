#include "erelia/client/command/connect_command.hpp"

#include "erelia/client/console_entry.hpp"

#include <charconv>
#include <cstdint>
#include <limits>
#include <string>

ConnectCommand::ConnectCommand(ConsoleEntry &owner) :
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

void ConnectCommand::execute(const spk::CommandParser::Invocation &invocation)
{
	ConsoleEntry::ConnectRequest request;

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
