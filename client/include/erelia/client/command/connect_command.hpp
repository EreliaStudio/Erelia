#pragma once

#include <cstdint>
#include <functional>
#include <optional>
#include <string>

#include <design_pattern/contract_provider.hpp>
#include <system/command_parser.hpp>

class ConnectCommand final : public spk::CommandParser::Command
{
public:
	struct Request
	{
		std::optional<std::string> address;
		std::optional<std::uint16_t> port;
	};

	using RequestProvider = spk::ContractProvider<const Request &>;
	using RequestCallback = RequestProvider::callback_type;
	using RequestContract = RequestProvider::Contract;
	using LocalOutputCallback = std::function<void(std::string)>;

private:
	LocalOutputCallback _localOutput;
	RequestProvider _requestProvider;

public:
	explicit ConnectCommand(LocalOutputCallback localOutput);

	void execute(const spk::CommandParser::Invocation &invocation) override;
	[[nodiscard]] RequestContract subscribeToRequest(RequestCallback callback);
};
