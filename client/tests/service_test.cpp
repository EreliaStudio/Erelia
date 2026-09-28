#include "erelia/client/service.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>
#include <system/translator.hpp>
#include <type/uuid.hpp>

#include <filesystem>
#include <fstream>
#include <string>

namespace
{
	class TemporaryTranslationFile final
	{
	private:
		std::filesystem::path _path;

	public:
		explicit TemporaryTranslationFile(
			const std::string &content)
		{
			_path =
				std::filesystem::temp_directory_path() /
				("erelia-client-translator-" +
				 spk::UUID::generate().toString() +
				 ".json");

			std::ofstream stream(_path);
			stream << content;
		}

		~TemporaryTranslationFile()
		{
			std::error_code error;
			std::filesystem::remove(_path, error);
		}

		[[nodiscard]] const std::filesystem::path &path() const noexcept
		{
			return _path;
		}
	};

	class ClientTranslatorServiceTest : public testing::Test
	{
	protected:
		void SetUp() override
		{
			Service::translator().clear();
		}

		void TearDown() override
		{
			Service::translator().clear();
		}
	};
}

TEST_F(ClientTranslatorServiceTest, ProvidesStableTranslatorInstance)
{
	EXPECT_EQ(
		&Service::translator(),
		&Service::translator());
}

TEST_F(ClientTranslatorServiceTest, DirectAppendPersistsAcrossServiceLookups)
{
	Service::translator().append(
		"client.message",
		"Message");

	EXPECT_EQ(
		Service::translator().translate("client.message"),
		"Message");
}

TEST_F(ClientTranslatorServiceTest, FormattingWorksThroughClientService)
{
	Service::translator().append(
		"client.connection.maximum_attempts_reached",
		"Unable to connect after {} attempts");

	EXPECT_EQ(
		Service::translator().translate(
			"client.connection.maximum_attempts_reached",
			5),
		"Unable to connect after 5 attempts");
}

TEST_F(ClientTranslatorServiceTest, FileAppendLoadsCatalogThroughClientService)
{
	const TemporaryTranslationFile file(
		R"({"client.first":"First","client.second":"Second {}"})");

	Service::translator().append(file.path());

	EXPECT_EQ(
		Service::translator().translate("client.first"),
		"First");
	EXPECT_EQ(
		Service::translator().translate("client.second", 2),
		"Second 2");
}

TEST_F(ClientTranslatorServiceTest, DuplicateDirectKeyIsRejected)
{
	Service::translator().append(
		"client.status",
		"Initial");

	EXPECT_THROW(
		Service::translator().append(
			"client.status",
			"Updated"),
		spk::Exception);
	EXPECT_EQ(
		Service::translator().translate("client.status"),
		"Initial");
}

TEST_F(ClientTranslatorServiceTest, FileAppendRejectsDuplicateKeyWithoutPartialMutation)
{
	const TemporaryTranslationFile file(
		R"({"client.shared":"From file","client.added":"Added"})");

	Service::translator().append(
		"client.shared",
		"Initial");
	Service::translator().append(
		"client.preserved",
		"Preserved");

	EXPECT_THROW(
		Service::translator().append(file.path()),
		spk::Exception);
	EXPECT_EQ(
		Service::translator().translate("client.shared"),
		"Initial");
	EXPECT_EQ(
		Service::translator().translate("client.preserved"),
		"Preserved");
	EXPECT_EQ(
		Service::translator().translate("client.added"),
		"client.added");
}

TEST_F(ClientTranslatorServiceTest, ClearFallsBackToKeyForEveryServiceLookup)
{
	Service::translator().append(
		"client.message",
		"Message");

	Service::translator().clear();

	EXPECT_EQ(
		Service::translator().translate("client.message"),
		"client.message");
}

TEST_F(ClientTranslatorServiceTest, TranslatorCanBeRepopulatedAfterClear)
{
	Service::translator().append(
		"client.message",
		"Before");

	Service::translator().clear();
	Service::translator().append(
		"client.message",
		"After");

	EXPECT_EQ(
		Service::translator().translate("client.message"),
		"After");
}

TEST_F(ClientTranslatorServiceTest, ClearThenAppendSupportsFullLanguageReplacement)
{
	const TemporaryTranslationFile english(
		R"({"client.common":"English","client.english_only":"English only"})");
	const TemporaryTranslationFile french(
		R"({"client.common":"Français","client.french_only":"Français seulement"})");

	Service::translator().append(english.path());

	EXPECT_EQ(
		Service::translator().translate("client.common"),
		"English");
	EXPECT_EQ(
		Service::translator().translate("client.english_only"),
		"English only");

	Service::translator().clear();
	Service::translator().append(french.path());

	EXPECT_EQ(
		Service::translator().translate("client.common"),
		"Français");
	EXPECT_EQ(
		Service::translator().translate("client.french_only"),
		"Français seulement");
	EXPECT_EQ(
		Service::translator().translate("client.english_only"),
		"client.english_only");
}

TEST_F(ClientTranslatorServiceTest, FailedCatalogAppendKeepsCurrentClientTranslations)
{
	const TemporaryTranslationFile invalid(
		R"({"client.valid":"Would be added","client.invalid":42})");

	Service::translator().append(
		"client.existing",
		"Existing");

	EXPECT_THROW(
		Service::translator().append(invalid.path()),
		spk::Exception);
	EXPECT_EQ(
		Service::translator().translate("client.existing"),
		"Existing");
	EXPECT_EQ(
		Service::translator().translate("client.valid"),
		"client.valid");
}

TEST_F(ClientTranslatorServiceTest, FailedFormatPropagatesThroughClientService)
{
	Service::translator().append(
		"client.invalid",
		"Value {");

	EXPECT_THROW(
		(void)Service::translator().translate(
			"client.invalid",
			42),
		spk::Exception);
}

TEST_F(ClientTranslatorServiceTest, MissingTranslationFallsBackToKey)
{
	EXPECT_EQ(
		Service::translator().translate(
			"client.connection.missing",
			42),
		"client.connection.missing");
}
