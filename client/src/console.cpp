#include "erelia/client/console.hpp"

#include "erelia/client/connection_manager.hpp"

#include <core/context/update_context.hpp>

#include <utility>

namespace
{
	[[nodiscard]] const char *levelName(spk::Logger::Level level) noexcept
	{
		switch (level)
		{
		case spk::Logger::Level::Trace:
			return "Trace";
		case spk::Logger::Level::Info:
			return "Info";
		case spk::Logger::Level::Warning:
			return "Warning";
		case spk::Logger::Level::Error:
			return "Error";
		}
		return "Unknown";
	}
}

Console::CommandEdit::CommandEdit(
	std::string name,
	Console &console,
	spk::Widget *parent) :
	spk::TextEdit(std::move(name), parent),
	_console(console)
{
}

void Console::CommandEdit::_onKeyPressedEvent(spk::KeyPressedEvent &event)
{
	if (
		event.record.key == spk::Keyboard::Return &&
		isFocused() == true)
	{
		const std::string command = textAsUTF8();
		setText("");
		_console.submit(command);
		event.consumed = true;
		return;
	}

	spk::TextEdit::_onKeyPressedEvent(event);
}

Console::Console(
	std::string name,
	ConnectionManager &connectionManager,
	spk::Widget *parent) :
	spk::Widget(std::move(name), parent),
	_connectionManager(connectionManager),
	_entries("Console.entries", this),
	_commandEdit("Console.command", *this, this),
	_loggerContract(spk::logger.subscribeToEntry(
		[this](const spk::Logger::Level &level, const std::string &message) {
			_queueEntry(level, message);
		}))
{
	_commandEdit.setPlaceholder("Enter text or /connect");
	_layout.addWidget(&_entries);
	_layout.addWidget(
		&_commandEdit,
		{spk::Layout::SizePolicy::Extend, spk::Layout::SizePolicy::Fixed});
	activate();
}

void Console::_queueEntry(
	const spk::Logger::Level &level,
	const std::string &message)
{
	const std::scoped_lock lock(_pendingMutex);
	_pendingEntries.emplace_back(
		"[" + std::string(levelName(level)) + "] " + message);
}

void Console::_flushEntries()
{
	std::vector<std::string> entries;
	{
		const std::scoped_lock lock(_pendingMutex);
		entries.swap(_pendingEntries);
	}

	if (entries.empty() == true)
	{
		return;
	}

	for (const std::string &entry : entries)
	{
		if (_displayedEntries.empty() == false)
		{
			_displayedEntries += '\n';
		}
		_displayedEntries += entry;
	}
	_entries.contentObject().setText(_displayedEntries);
}

void Console::_onGeometryChange()
{
	_layout.setGeometry(geometry());
}

void Console::_updateState(spk::UpdateContext &)
{
	_flushEntries();
}

void Console::submit(std::string command)
{
	if (command.empty() == true)
	{
		return;
	}

	if (command == "/connect")
	{
		SPK_LOG(Info) << "Console command: /connect" << std::endl;
		_connectionManager.connect();
		return;
	}

	SPK_LOG(Info) << command << std::endl;
}
