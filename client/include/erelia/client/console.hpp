#pragma once

#include <string>

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
	struct PendingEntry
	{
		spk::Logger::Level level;
		std::string message;
	};

	spk::DataModel<std::string> _entryModel;
	spk::Panel _entryBackground;
	spk::TextModelView _entries;
	ConsoleEntry _commandEntry;
	spk::VerticalLayout _layout;
	spk::Logger::OnEntryContract _loggerContract;
	spk::ThreadSafeFIFO<PendingEntry> _pendingEntries;

	void _queueEntry(const spk::Logger::Level &level, const std::string &message);
	void _flushEntries();

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
	[[nodiscard]] spk::CommandParser &commandParser() noexcept;
	[[nodiscard]] const spk::CommandParser &commandParser() const noexcept;
	[[nodiscard]] ConsoleEntry &commandEntry() noexcept;
	[[nodiscard]] const ConsoleEntry &commandEntry() const noexcept;
};
