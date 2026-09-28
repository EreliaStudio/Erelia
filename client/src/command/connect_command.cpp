#include "erelia/client/command/connect_command.hpp"

#include "erelia/client/service.hpp"

#include <diagnostics/logger.hpp>
#include <system/translator.hpp>

#include <charconv>
#include <cstdint>
#include <limits>
#include <string>
#include <utility>

ConnectCommand::ConnectCommand() :
	Command(
		"connect",
		Service::translator().translate(
			"client.command.connect.description"),
		{
			{.name = "address",
			 .description = Service::translator().translate(
				 "client.command.connect.address.description"),
			 .optional = true},
			{.name = "port",
			 .description = Service::translator().translate(
				 "client.command.connect.port.description"),
			 .optional = true},
		})
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
			SPK_LOG(UserValueB)
				<< Service::translator().translate(
					   "client.command.connect.invalid_port",
					   value)
				<< std::endl;
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
