#pragma once

#include "erelia/client/command/connect_command.hpp"

#include <string>

#include <design_pattern/contract_provider.hpp>
#include <system/command_parser.hpp>
#include <ui/widget/text_edit.hpp>

class ConsoleEntry final : public spk::TextEdit
{
public:
	using SubmissionProvider = spk::ContractProvider<std::string>;
	using SubmissionCallback = SubmissionProvider::callback_type;
	using SubmissionContract = SubmissionProvider::Contract;

private:
	spk::CommandParser _commandParser;
	SubmissionProvider _submissionProvider;
	ConnectCommand &_connectCommand;

	void _emitLocal(std::string message);
	void _emitFailure(const spk::CommandParser::Result &result);

public:
	ConsoleEntry(
		std::string name,
		spk::Widget *parent = nullptr);

	void submit(std::string input);
	[[nodiscard]] SubmissionContract subscribeToSubmission(SubmissionCallback callback);
	[[nodiscard]] ConnectCommand::RequestContract subscribeToConnectRequest(ConnectCommand::RequestCallback callback);
	[[nodiscard]] spk::CommandParser &commandParser() noexcept;
	[[nodiscard]] const spk::CommandParser &commandParser() const noexcept;
};
