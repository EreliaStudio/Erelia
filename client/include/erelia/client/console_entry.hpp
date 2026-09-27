#pragma once

#include <cstdint>
#include <functional>
#include <optional>
#include <string>

#include <design_pattern/contract_provider.hpp>
#include <system/command_parser.hpp>
#include <ui/widget/text_edit.hpp>

class ConsoleEntry final : public spk::TextEdit
{
public:
	using LocalOutputCallback = std::function<void(std::string)>;

	struct ConnectRequest
	{
		std::optional<std::string> address;
		std::optional<std::uint16_t> port;
	};

	using ConnectRequestProvider = spk::ContractProvider<const ConnectRequest &>;
	using ConnectRequestCallback = ConnectRequestProvider::callback_type;
	using ConnectRequestContract = ConnectRequestProvider::Contract;

private:
	spk::CommandParser _commandParser;
	LocalOutputCallback _localOutput;
	ConnectRequestProvider _connectRequestProvider;

	void _emitLocal(std::string message);
	void _emitFailure(const spk::CommandParser::Result &result);
	void _registerCommands();

public:
	ConsoleEntry(
		std::string name,
		LocalOutputCallback localOutput,
		spk::Widget *parent = nullptr);

	void submit(std::string input);
	[[nodiscard]] ConnectRequestContract subscribeToConnectRequest(ConnectRequestCallback callback);
	[[nodiscard]] spk::CommandParser &commandParser() noexcept;
	[[nodiscard]] const spk::CommandParser &commandParser() const noexcept;
};
