#pragma once

#include <string>

#include "erelia/client/command/connect_command.hpp"
#include "erelia/client/console_entry.hpp"
#include <container/data_model.hpp>
#include <container/thread_safe_fifo.hpp>
#include <diagnostics/logger.hpp>
#include <ui/layout/linear_layout.hpp>
#include <ui/text_model_view.hpp>
#include <ui/widget.hpp>
#include <ui/widget/panel.hpp>

class Console final : public spk::Widget
{
private:
	spk::DataModel<std::string> _entryModel;
	spk::Panel _entryBackground;
	spk::TextModelView _entries;
	ConsoleEntry _commandEntry;
	spk::VerticalLayout _layout;
	spk::Logger::OnEntryContract _loggerContract;
	spk::ThreadSafeFIFO<std::string> _pendingEntries;

	void _queueEntry(const spk::Logger::Level &level, const std::string &message);
	void _flushEntries();
	void _appendLocalEntry(std::string entry);

public:
	[[nodiscard]] const spk::DataModel<std::string> &entries() const noexcept;
	[[nodiscard]] spk::TextModelView &entryView() noexcept;
	[[nodiscard]] const spk::TextModelView &entryView() const noexcept;

private:
	void _updateSizeHint() override;
	void _onGeometryChange() override;
	void _updateState(spk::UpdateContext &context) override;
	void _onPassiveKeyPressedEvent(spk::KeyPressedEvent &event) override;

public:
	Console(std::string name, spk::Widget *parent = nullptr);

	void submit(std::string command);
	[[nodiscard]] ConnectCommand::RequestContract subscribeToConnectRequest(ConnectCommand::RequestCallback callback);
	[[nodiscard]] ConsoleEntry &commandEntry() noexcept;
	[[nodiscard]] const ConsoleEntry &commandEntry() const noexcept;
};
