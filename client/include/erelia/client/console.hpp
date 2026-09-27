#pragma once

#include <mutex>
#include <string>
#include <vector>

#include <diagnostics/logger.hpp>
#include <ui/layout/linear_layout.hpp>
#include <ui/widget/scroll_area.hpp>
#include <ui/widget/text_area.hpp>
#include <ui/widget/text_edit.hpp>
#include <ui/widget.hpp>

class ConnectionManager;

class Console final : public spk::Widget
{
private:
	class CommandEdit final : public spk::TextEdit
	{
	private:
		Console &_console;
		void _onKeyPressedEvent(spk::KeyPressedEvent &event) override;

	public:
		CommandEdit(std::string name, Console &console, spk::Widget *parent);
	};

	ConnectionManager &_connectionManager;
	spk::ScrollArea<spk::TextArea> _entries;
	CommandEdit _commandEdit;
	spk::VerticalLayout _layout;
	spk::Logger::OnEntryContract _loggerContract;
	std::mutex _pendingMutex;
	std::vector<std::string> _pendingEntries;
	std::string _displayedEntries;

	void _queueEntry(const spk::Logger::Level &level, const std::string &message);
	void _flushEntries();
	void _onGeometryChange() override;
	void _updateState(spk::UpdateContext &context) override;

public:
	Console(std::string name, ConnectionManager &connectionManager, spk::Widget *parent = nullptr);

	void submit(std::string command);
};
