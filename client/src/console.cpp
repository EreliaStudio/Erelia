#include "erelia/client/console.hpp"

#include "erelia/client/service.hpp"

#include <core/context/update_context.hpp>
#include <system/translator.hpp>

#include <utility>
#include <vector>

Console::Console(
	std::string name,
	spk::Widget *parent) :
	spk::Widget(std::move(name), parent),
	_entryBackground(this->name() + "/entries/background", this),
	_entries(this->name() + "/entries", &_entryModel, this),
	_commandEntry(this->name() + "/command", this),
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
	if (
		level != spk::Logger::Level::UserValueA &&
		level != spk::Logger::Level::UserValueB)
	{
		return;
	}

	_pendingEntries.publish(
		PendingEntry{
			.level = level,
			.message = message});
}

void Console::_flushEntries()
{
	std::vector<PendingEntry> entries;
	(void)_pendingEntries.drain(entries);

	if (entries.empty() == true)
	{
		return;
	}

	const bool followTail =
		_entryModel.empty() == true ||
		_entries.isLastRowVisible() == true;

	for (PendingEntry &entry : entries)
	{
		const std::string prefix =
			entry.level == spk::Logger::Level::UserValueA
				? Service::translator().translate(
					  "client.console.user_label") +
					  " : "
				: "[" +
					  Service::translator().translate(
						  "client.console.command_label") +
					  "] : ";

		std::size_t begin = 0;
		while (begin < entry.message.size())
		{
			const std::size_t end =
				entry.message.find('\n', begin);

			_entryModel.append(
				prefix +
				entry.message.substr(
					begin,
					end - begin));

			if (end == std::string::npos)
			{
				break;
			}
			begin = end + 1;
		}
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

void Console::submit(std::string command)
{
	_commandEntry.submit(std::move(command));
}

spk::CommandParser &Console::commandParser() noexcept
{
	return _commandEntry.commandParser();
}

const spk::CommandParser &Console::commandParser() const noexcept
{
	return _commandEntry.commandParser();
}

ConsoleEntry &Console::commandEntry() noexcept
{
	return _commandEntry;
}

const ConsoleEntry &Console::commandEntry() const noexcept
{
	return _commandEntry;
}
