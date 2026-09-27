#include "erelia/client/console.hpp"

#include <core/context/update_context.hpp>

#include <utility>
#include <vector>

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
	spk::Widget *parent) :
	spk::Widget(std::move(name), parent),
	_entryBackground("Console.entries.background", this),
	_entries("Console.entries", &_entryModel, this),
	_commandEntry("Console.command", this),
	_submissionContract(_commandEntry.subscribeToSubmission([this](std::string entry) {
		_appendLocalEntry(std::move(entry));
	})),
	_loggerContract(spk::logger.subscribeToEntry([this](const spk::Logger::Level &level, const std::string &message) {
		_queueEntry(level, message);
	}))
{
	_entryBackground.setZOrder(0.0f);
	_entries.setZOrder(1.0f);
	_layout.addWidget(&_entryBackground);
	_layout.addWidget(
		&_commandEntry,
		{spk::Layout::SizePolicy::Extend, spk::Layout::SizePolicy::Fixed});
	_updateSizeHint();
	activate();
}

void Console::_queueEntry(
	const spk::Logger::Level &level,
	const std::string &message)
{
	if (level == spk::Logger::Level::UserValueA)
	{
		_pendingEntries.emplace("User : " + message);
	}
	else
	{
		_pendingEntries.emplace(
			"[" + std::string(levelName(level)) + "] : " + message);
	}
}

void Console::_flushEntries()
{
	std::vector<std::string> entries;
	_pendingEntries.drain(entries);

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

void Console::_updateSizeHint()
{
	setSizeHint(_layout.sizeHint());
}

void Console::_onGeometryChange()
{
	_layout.setGeometry(spk::Rect2D{.anchor = {0, 0}, .size = geometry().size});
	_entries.setGeometry(spk::Rect2D{.anchor = {0, 0}, .size = _entryBackground.geometry().size});
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
	std::size_t begin = 0;
	while (begin < entry.size())
	{
		const std::size_t end = entry.find('\n', begin);
		_entryModel.append(entry.substr(begin, end - begin));
		if (end == std::string::npos)
		{
			break;
		}
		begin = end + 1;
	}

	if (followTail == true && _entryModel.empty() == false)
	{
		_entries.scrollTo(_entryModel.rowCount() - 1);
	}
}

void Console::submit(std::string command)
{
	_commandEntry.submit(std::move(command));
}

ConnectCommand::RequestContract Console::subscribeToConnectRequest(ConnectCommand::RequestCallback callback)
{
	return _commandEntry.subscribeToConnectRequest(std::move(callback));
}

ConsoleEntry &Console::commandEntry() noexcept
{
	return _commandEntry;
}

const ConsoleEntry &Console::commandEntry() const noexcept
{
	return _commandEntry;
}
