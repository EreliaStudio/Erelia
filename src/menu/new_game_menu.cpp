#include "menu/new_game_menu.hpp"

#include "structure/world.hpp"

void NewGameMenu::Content::SeedField::_onGeometryChange()
{
	_generateButton.setMinimalSize(geometry().size.y);
	_layout.setGeometry({0, geometry().size});
}

NewGameMenu::Content::SeedField::SeedField(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::Widget(p_name, p_parent),
	_field(p_name + L"/Field", this),
	_generateButton(p_name + L"/GenerateButton", this)
{
	_seedGenerator.configureRange(33, 126);

	_field.setPlaceholder(L"Enter seed value");
	_field.activate();

	_generateButton.setOnClick([&](){
		generateSeed();
	});
	spk::SafePointer<spk::SpriteSheet> assetAtlas = AssetAtlas::instance()->spriteSheet(L"iconSet");
	_generateButton.setIconset(assetAtlas);
	_generateButton.setIcon(assetAtlas->sprite(0));
	_generateButton.activate();

	_layout.setElementPadding(10);
	_layout.addWidget(&_field, spk::Layout::SizePolicy::Extend);
	_layout.addWidget(&_generateButton, spk::Layout::SizePolicy::Minimum);
}

void NewGameMenu::Content::SeedField::generateSeed()
{
	std::wstring newSeed;
	for (size_t i = 0; i < 8; ++i)
	{
		newSeed.push_back(static_cast<wchar_t>(_seedGenerator.sample()));
	}
	_field.setText(newSeed);
}

spk::TextEdit& NewGameMenu::Content::SeedField::field()
{
	return _field;
}

PushButton& NewGameMenu::Content::SeedField::generateButton()
{
	return (_generateButton);
}

const std::wstring& NewGameMenu::Content::SeedField::seed() const
{
	return (_field.text());
}

void NewGameMenu::Content::_onGeometryChange()
{
	_layout.setGeometry({0, geometry().size});
}

NewGameMenu::Content::Content(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::Widget(p_name, p_parent),
	_nameRow(p_name + L"/NameRow", this),
	_seedRow(p_name + L"/SeedRow", this),
	_spacer(p_name + L"/Spacer", this)
{
	WidgetAddons::ApplyFormat(&_nameRow.label);
	WidgetAddons::ApplyFormat(&_nameRow.field);
	WidgetAddons::ApplyFormat(&_seedRow.label);

	_nameRow.label.setText(L"Name:");
	_nameRow.field.setText(L"New game");
	_nameRow.activate();

	_seedRow.label.setText(L"Seed:");
	_seedRow.field.generateSeed();
	_seedRow.activate();

	_layout.setElementPadding(10);
	_layout.addRow(&_nameRow, spk::Layout::SizePolicy::Minimum, spk::Layout::SizePolicy::HorizontalExtend);
	_layout.addRow(&_seedRow, spk::Layout::SizePolicy::Minimum, spk::Layout::SizePolicy::HorizontalExtend);
	_layout.addRow(nullptr, &_spacer, spk::Layout::SizePolicy::Minimum, spk::Layout::SizePolicy::Extend);
}

const std::wstring& NewGameMenu::Content::name() const
{
	return (_nameRow.field.text());
}

const std::wstring& NewGameMenu::Content::seed() const
{
	return (_seedRow.field.seed());
}

void NewGameMenu::_onGeometryChange()
{
	WidgetAddons::centerInParent(&_layout, spk::Vector2Int::clamp({200, 110}, geometry().size / 2, {500, 320}), geometry());

	WidgetAddons::centerInParent(&_saveConfirmationRequestMessageBox, spk::Vector2Int::clamp({200, 110}, geometry().size / 2, {300, 220}), geometry());
}

NewGameMenu::NewGameMenu(const std::wstring& p_name, spk::SafePointer<spk::Widget> p_parent) :
	spk::Screen(p_name, p_parent),
	_commandPanel(p_name + L"/CommandPanel", this),
	_content(p_name + L"/Content", this),
	_saveConfirmationRequestMessageBox(p_name + L"/SaveConfirmationRequestMessageBox", this)
{
	_saveConfirmationRequestMessageBox.setLayer(100);
	_saveConfirmationRequestMessageBox.setText(L"A save with the same name already exists. Do you want to overwrite it?");
	_saveConfirmationRequestMessageBox.configure(L"Override", [this](){
		_onConfirmLambda();
	},
	L"Cancel", [&](){
		_saveConfirmationRequestMessageBox.deactivate();
	});

	_content.activate();

	_confirmButton = _commandPanel.addButton(L"Confirm", L"Confirm", [this]() {});
	_commandPanel.setOnClick(L"Confirm", [&](){
		if (GameFile::instance()->isSavable() == true)
		{
			_onConfirmLambda();
		}
		else
		{
			_saveConfirmationRequestMessageBox.activate();
		}
	});

	_cancelButton = _commandPanel.addButton(L"Cancel", L"Cancel", [this]() {});

	WidgetAddons::ApplyFormat(_confirmButton);
	WidgetAddons::ApplyFormat(_cancelButton);

	_commandPanel.activate();

	_layout.setElementPadding(10);
	_layout.addWidget(&_content);
	_layout.addWidget(&_commandPanel, spk::Layout::SizePolicy::Minimum);
}

const std::wstring& NewGameMenu::name() const
{
	return (_content.name());
}

const std::wstring& NewGameMenu::seed() const
{
	return (_content.seed());
}

void NewGameMenu::onConfirmRequest(const spk::PushButton::Job& p_job)
{
	_onConfirmLambda = p_job;
}

void NewGameMenu::onCancelRequest(const spk::PushButton::Job& p_job)
{
	_commandPanel.setOnClick(L"Cancel", p_job);
}