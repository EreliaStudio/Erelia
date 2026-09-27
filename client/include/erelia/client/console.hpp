#pragma once

#include <mutex>
#include <string>
#include <vector>

#include <diagnostics/logger.hpp>
#include <ui/layout/linear_layout.hpp>
#include <container/data_model.hpp>
#include <ui/text_model_view.hpp>
#include <ui/widget/text_edit.hpp>
#include <ui/widget.hpp>

class ConnectionManager;

class Console final : public spk::Widget
{
private:
	ConnectionManager &_connectionManager;
	spk::DataModel<std::string> _entryModel;
	spk::TextModelView _entries;
	spk::TextEdit _commandEdit;
	spk::VerticalLayout _layout;
	spk::Logger::OnEntryContract _loggerContract;
	std::mutex _pendingMutex;
	std::vector<std::string> _pendingEntries;
	bool _followTail = true;

	void _queueEntry(const spk::Logger::Level &level, const std::string &message);
	void _flushEntries();

public:
	[[nodiscard]] const spk::DataModel<std::string> &entries() const noexcept;
	[[nodiscard]] spk::TextModelView &entryView() noexcept;
	[[nodiscard]] const spk::TextModelView &entryView() const noexcept;

private:
	void _onGeometryChange() override;
	void _updateState(spk::UpdateContext &context) override;
	void _onPassiveKeyPressedEvent(spk::KeyPressedEvent &event) override;

public:
	Console(std::string name, ConnectionManager &connectionManager, spk::Widget *parent = nullptr);

	void submit(std::string command);
};
