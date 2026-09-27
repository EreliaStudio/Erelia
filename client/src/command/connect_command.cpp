#include "erelia/client/command/connect_command.hpp"

#include <charconv>
#include <cstdint>
#include <limits>
#include <string>
#include <utility>

ConnectCommand::ConnectCommand(LocalOutputCallback localOutput) :
	Command(
		"connect",
		"Starts a new dedicated Server connection cycle.",
		{
			{.name = "address", .description = "Dedicated Server address", .optional = true},
			{.name = "port", .description = "Dedicated Server port", .optional = true},
		}),
	_localOutput(std::move(localOutput))
{
}

void ConnectCommand::execute(const spk::CommandParser::Invocation &invocation)
{
	Request request;

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
			if (_localOutput)
			{
				_localOutput("Invalid port: " + value);
			}
			return;
		}
		request.port = static_cast<std::uint16_t>(port);
	}

	_requestProvider.trigger(request);
}

ConnectCommand::RequestContract ConnectCommand::subscribeToRequest(RequestCallback callback)
{
	return _requestProvider.subscribe(std::move(callback));
}
