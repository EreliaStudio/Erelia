#include "erelia/core/service.hpp"
#include "erelia/core/translation_engine.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>
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
		explicit TemporaryTranslationFile(const std::string &content)
		{
			_path =
				std::filesystem::temp_directory_path() /
				("erelia-translations-" + spk::UUID::generate().toString() + ".json");

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
}

TEST(TranslationEngineTest, LoadsAndFormatsTranslations)
{
	const TemporaryTranslationFile file(
		R"({"client.connection.maximum_attempts_reached":"Unable to connect after {} attempts","ordered":"{1} then {0}"})");

	TranslationEngine engine;
	engine.load(file.path());

	EXPECT_EQ(
		engine.translate(
			"client.connection.maximum_attempts_reached",
			5),
		"Unable to connect after 5 attempts");
	EXPECT_EQ(
		engine.translate("ordered", "first", "second"),
		"second then first");
}

TEST(TranslationEngineTest, ReloadReplacesTheActiveCatalog)
{
	const TemporaryTranslationFile first(
		R"({"message":"First {}"})");
	const TemporaryTranslationFile second(
		R"({"message":"Second {}"})");

	TranslationEngine engine;
	engine.load(first.path());
	EXPECT_EQ(engine.translate("message", 1), "First 1");

	engine.load(second.path());
	EXPECT_EQ(engine.translate("message", 2), "Second 2");
}

TEST(TranslationEngineTest, FailedLoadKeepsThePreviousCatalog)
{
	const TemporaryTranslationFile valid(
		R"({"message":"Valid {}"})");
	const TemporaryTranslationFile invalid(
		R"({"message":42})");

	TranslationEngine engine;
	engine.load(valid.path());

	EXPECT_THROW(engine.load(invalid.path()), spk::Exception);
	EXPECT_EQ(engine.translate("message", "catalog"), "Valid catalog");
}

TEST(TranslationEngineTest, UnknownIdentifierIsRejected)
{
	const TemporaryTranslationFile file(
		R"({"known":"Known"})");

	TranslationEngine engine;
	engine.load(file.path());

	EXPECT_THROW(
		(void)engine.translate("unknown"),
		spk::Exception);
}

TEST(TranslationEngineTest, InvalidFormatIsRejectedAtTranslationTime)
{
	const TemporaryTranslationFile file(
		R"({"invalid":"Value {"})");

	TranslationEngine engine;
	engine.load(file.path());

	EXPECT_THROW(
		(void)engine.translate("invalid", 1),
		spk::Exception);
}

TEST(TranslationEngineTest, ServiceProvidesStableEngineInstance)
{
	EXPECT_EQ(
		Service::translationEngine(),
		Service::translationEngine());
}
