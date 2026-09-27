#pragma once

#include <mutex>
#include <string>
#include <vector>

#include "erelia/client/console_entry.hpp"
#include <container/data_model.hpp>
#include <diagnostics/logger.hpp>
#include <ui/layout/linear_layout.hpp>
#include <ui/text_model_view.hpp>
#include <ui/widget.hpp>

class ConnectionManager;

class Console final : public spk::Widget
{
private:
	spk::DataModel<std::string> _entryModel;
	spk::TextModelView _entries;
	ConsoleEntry _commandEntry;
	spk::VerticalLayout _layout;
	spk::Logger::OnEntryContract _loggerContract;
	std::mutex _pendingMutex;
	std::vector<std::string> _pendingEntries;

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
	Console(std::string name, ConnectionManager &connectionManager, spk::Widget *parent = nullptr);

	void submit(std::string command);
	[[nodiscard]] ConsoleEntry &commandEntry() noexcept;
	[[nodiscard]] const ConsoleEntry &commandEntry() const noexcept;
};
