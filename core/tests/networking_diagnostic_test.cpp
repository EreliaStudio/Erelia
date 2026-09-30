#include "erelia/core/networking/diagnostic.hpp"
#include "erelia/core/networking/message_type.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <cstdint>
#include <string>
#include <utility>

namespace
{
	[[nodiscard]] spk::Message::Type diagnosticMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(
			Networking::MessageType::Diagnostic);
	}

	Networking::Diagnostic buildDiagnostic(
		Networking::Diagnostic::Severity severity,
		std::string message,
		spk::Message::RequestID requestID = 0u)
	{
		Networking::Diagnostic::Builder builder(
			severity,
			std::move(message),
			requestID);
		return std::move(builder).build();
	}
}

TEST(NetworkingDiagnostic, BuilderProducesUncorrelatedDiagnosticByDefault)
{
	const auto diagnostic = buildDiagnostic(
		Networking::Diagnostic::Severity::Warning,
		"Chunk_Coordinates_Duplication");

	EXPECT_EQ(diagnostic.type(), diagnosticMessageType());
	EXPECT_EQ(diagnostic.requestID(), 0u);
	EXPECT_EQ(
		diagnostic.severity(),
		Networking::Diagnostic::Severity::Warning);
	EXPECT_EQ(
		diagnostic.message(),
		"Chunk_Coordinates_Duplication");
}

TEST(NetworkingDiagnostic, BuilderPreservesCorrelation)
{
	const auto diagnostic = buildDiagnostic(
		Networking::Diagnostic::Severity::Error,
		"Chunk_Request_Malformed",
		71u);

	EXPECT_EQ(diagnostic.requestID(), 71u);
	EXPECT_EQ(
		diagnostic.severity(),
		Networking::Diagnostic::Severity::Error);
	EXPECT_EQ(
		diagnostic.message(),
		"Chunk_Request_Malformed");
}

TEST(NetworkingDiagnostic, UsesSeverityThenSparkleStringEncoding)
{
	const std::string key = "Diagnostic_Key";
	const auto diagnostic = buildDiagnostic(
		Networking::Diagnostic::Severity::Info,
		key);

	ASSERT_EQ(
		diagnostic.size(),
		sizeof(std::uint8_t) +
			sizeof(std::uint32_t) +
			key.size());
	EXPECT_EQ(
		diagnostic.reader().readAt<std::uint8_t>(0u),
		static_cast<std::uint8_t>(
			Networking::Diagnostic::Severity::Info));
	EXPECT_EQ(
		diagnostic.reader().readAt<std::uint32_t>(
			sizeof(std::uint8_t)),
		key.size());
}

TEST(NetworkingDiagnostic, RoundTripIsReaderIndependent)
{
	const auto source = buildDiagnostic(
		Networking::Diagnostic::Severity::Trace,
		"Trace_Key",
		19u);

	const spk::Message raw = source;
	auto externalReader = raw.reader();
	externalReader.skip<std::uint8_t>();
	const auto originalOffset = externalReader.readOffset();

	const Networking::Diagnostic decoded(raw);

	EXPECT_EQ(externalReader.readOffset(), originalOffset);
	EXPECT_EQ(decoded.requestID(), 19u);
	EXPECT_EQ(
		decoded.severity(),
		Networking::Diagnostic::Severity::Trace);
	EXPECT_EQ(decoded.message(), "Trace_Key");
}

TEST(NetworkingDiagnostic, SerializerReusesDiagnosticPrefix)
{
	const auto diagnostic = buildDiagnostic(
		Networking::Diagnostic::Severity::Warning,
		"Shared_Prefix",
		5u);

	spk::Message::Writer writer(99u);
	writer << diagnostic;
	const spk::Message destination =
		std::move(writer).build();

	EXPECT_EQ(
		destination.reader().readAt<std::uint8_t>(0u),
		static_cast<std::uint8_t>(
			Networking::Diagnostic::Severity::Warning));
	EXPECT_EQ(
		destination.reader().readAt<std::uint32_t>(
			sizeof(std::uint8_t)),
		std::string("Shared_Prefix").size());
}

TEST(NetworkingDiagnostic, RejectsWrongMessageType)
{
	const auto source = buildDiagnostic(
		Networking::Diagnostic::Severity::Info,
		"Key");
	spk::Message raw = source;
	spk::Message::Writer writer(std::move(raw));
	writer.setType(
		static_cast<spk::Message::Type>(
			Networking::MessageType::ChunkError));
	raw = std::move(writer).build();

	EXPECT_THROW(
		(void)Networking::Diagnostic(raw),
		spk::Exception);
}

TEST(NetworkingDiagnostic, RejectsUnknownSeverity)
{
	spk::Message::Writer writer(diagnosticMessageType());
	writer << std::uint8_t{99u};
	writer << std::string("Key");
	const spk::Message raw = std::move(writer).build();

	EXPECT_THROW(
		(void)Networking::Diagnostic(raw),
		spk::Exception);
}

TEST(NetworkingDiagnostic, RejectsTruncatedString)
{
	spk::Message::Writer writer(diagnosticMessageType());
	writer << static_cast<std::uint8_t>(
		Networking::Diagnostic::Severity::Error);
	writer << std::uint32_t{5u};
	writer.append("abc", 3u);
	const spk::Message raw = std::move(writer).build();

	EXPECT_THROW(
		(void)Networking::Diagnostic(raw),
		spk::Exception);
}

TEST(NetworkingDiagnostic, RejectsTrailingBytes)
{
	const auto source = buildDiagnostic(
		Networking::Diagnostic::Severity::Error,
		"Key");
	spk::Message raw = source;
	spk::Message::Writer writer(std::move(raw));
	writer << std::uint8_t{0u};
	raw = std::move(writer).build();

	EXPECT_THROW(
		(void)Networking::Diagnostic(raw),
		spk::Exception);
}
