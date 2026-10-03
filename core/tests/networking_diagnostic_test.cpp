#include "erelia/core/networking/diagnostic.hpp"
#include <exception.hpp>
#include <gtest/gtest.h>
TEST(NetworkingDiagnostic, PayloadRoundTripAndIndependentReaders)
{
	for (auto severity : {Networking::Diagnostic::Severity::Trace, Networking::Diagnostic::Severity::Info, Networking::Diagnostic::Severity::Warning, Networking::Diagnostic::Severity::Error})
	{
		spk::Message::Writer writer;
		writer << Networking::Diagnostic{severity, "Key"};
		auto message = std::move(writer).build();
		Networking::Diagnostic first, second;
		message.reader() >> first;
		message.reader() >> second;
		EXPECT_EQ(first.severity, severity);
		EXPECT_EQ(first.message, "Key");
		EXPECT_EQ(second.message, "Key");
	}
}
TEST(NetworkingDiagnostic, RejectsUnknownSeverityAndTruncation)
{
	spk::Message::Writer writer;
	writer << std::uint8_t{4} << std::string("Key");
	auto invalid = std::move(writer).build();
	Networking::Diagnostic value;
	EXPECT_THROW(invalid.reader() >> value, spk::Exception);
	spk::Message::Writer shortWriter;
	shortWriter << std::uint8_t{1};
	auto shortMessage = std::move(shortWriter).build();
	EXPECT_THROW(shortMessage.reader() >> value, spk::Exception);
}
