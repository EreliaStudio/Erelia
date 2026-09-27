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
		case spk::Logger::Level::UserValueA:
			return "User";
		case spk::Logger::Level::UserValueB:
			return "Command";
		case spk::Logger::Level::Warning:
			return "Warning";
		case spk::Logger::Level::Error:
			return "Error";
		}
		return "Unknown";
	}
}

Console::Console(
	std::string name,
	ConnectionManager &connectionManager,
	spk::Widget *parent) :
	spk::Widget(std::move(name), parent),
	_entries("Console.entries", &_entryModel, this),
	_commandEntry("Console.command", connectionManager, [this](std::string entry) { _appendLocalEntry(std::move(entry)); }, this),
	_loggerContract(spk::logger.subscribeToEntry(
		[this](const spk::Logger::Level &level, const std::string &message) {
			_queueEntry(level, message);
		}))
{
	_layout.addWidget(&_entries);
	_layout.addWidget(
		&_commandEntry,
		{spk::Layout::SizePolicy::Extend, spk::Layout::SizePolicy::Fixed});
	activate();
}

void Console::_queueEntry(
	const spk::Logger::Level &level,
	const std::string &message)
{
	const std::scoped_lock lock(_pendingMutex);
	if (level == spk::Logger::Level::UserValueA)
	{
		_pendingEntries.emplace_back("User : " + message);
	}
	else
	{
		_pendingEntries.emplace_back(
			"[" + std::string(levelName(level)) + "] : " + message);
	}
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

	const bool followTail =
		_entryModel.empty() == true ||
		_entries.isLastRowVisible() == true;

	for (std::string &entry : entries)
	{
		_entryModel.append(std::move(entry));
	}

	if (followTail == true && _entryModel.empty() == false)
	{
		_entries.scrollTo(_entryModel.rowCount() - 1);
	}
}

const spk::DataModel<std::string> &Console::entries() const noexcept
{
	return _entryModel;
}

spk::TextModelView &Console::entryView() noexcept
{
	return _entries;
}

const spk::TextModelView &Console::entryView() const noexcept
{
	return _entries;
}

void Console::_onGeometryChange()
{
	_layout.setGeometry(geometry());
}

void Console::_updateState(spk::UpdateContext &)
{
	_flushEntries();
}

void Console::_onPassiveKeyPressedEvent(spk::KeyPressedEvent &event)
{
	if (
		event.record.key == spk::Keyboard::Return &&
		_commandEntry.isFocused() == true)
	{
		const std::string command = _commandEntry.textAsUTF8();
		_commandEntry.setText("");
		submit(command);
		event.consumed = true;
	}
}

void Console::_appendLocalEntry(std::string entry)
{
	const bool followTail =
		_entryModel.empty() == true ||
		_entries.isLastRowVisible() == true;
	_entryModel.append(std::move(entry));
	if (followTail == true)
	{
		_entries.scrollTo(_entryModel.rowCount() - 1);
	}
}

void Console::submit(std::string command)
{
	_commandEntry.submit(std::move(command));
}

ConsoleEntry &Console::commandEntry() noexcept
{
	return _commandEntry;
}

const ConsoleEntry &Console::commandEntry() const noexcept
{
	return _commandEntry;
}
