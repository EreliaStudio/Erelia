#include "erelia/core/networking/diagnostic.hpp"
#include "erelia/core/networking/message_type.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <cstdint>
#include <utility>

namespace
{
	[[nodiscard]] spk::Message::Type diagnosticMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(
			Networking::MessageType::Diagnostic);
	}
}

TEST(NetworkingDiagnostic, BuilderProducesUncorrelatedDiagnosticByDefault)
{
	auto diagnostic = Networking::Diagnostic::Builder(
		Networking::Diagnostic::Severity::Warning,
		"Chunk_Coordinates_Duplication")
		.build();

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
	auto diagnostic = Networking::Diagnostic::Builder(
		Networking::Diagnostic::Severity::Error,
		"Chunk_Request_Malformed",
		71u)
		.build();

	EXPECT_EQ(diagnostic.requestID(), 71u);
	EXPECT_EQ(
		diagnostic.severity(),
		Networking::Diagnostic::Severity::Error);
	EXPECT_EQ(diagnostic.message(), "Chunk_Request_Malformed");
}

TEST(NetworkingDiagnostic, UsesSeverityThenSparkleStringEncoding)
{
	const std::string key = "Diagnostic_Key";
	auto diagnostic = Networking::Diagnostic::Builder(
		Networking::Diagnostic::Severity::Info,
		key)
		.build();

	ASSERT_EQ(
		diagnostic.size(),
		sizeof(std::uint8_t) +
			sizeof(std::uint32_t) +
			key.size());
	EXPECT_EQ(
		diagnostic.readAt<std::uint8_t>(0u),
		static_cast<std::uint8_t>(
			Networking::Diagnostic::Severity::Info));
	EXPECT_EQ(
		diagnostic.readAt<std::uint32_t>(sizeof(std::uint8_t)),
		key.size());
}

TEST(NetworkingDiagnostic, RoundTripIsCursorIndependent)
{
	auto source = Networking::Diagnostic::Builder(
		Networking::Diagnostic::Severity::Trace,
		"Trace_Key",
		19u)
		.build();

	spk::Message raw = source;
	raw.skip<std::uint8_t>();
	const auto originalOffset = raw.readOffset();

	Networking::Diagnostic decoded(raw);

	EXPECT_EQ(raw.readOffset(), originalOffset);
	EXPECT_EQ(decoded.readOffset(), originalOffset);
	EXPECT_EQ(decoded.requestID(), 19u);
	EXPECT_EQ(
		decoded.severity(),
		Networking::Diagnostic::Severity::Trace);
	EXPECT_EQ(decoded.message(), "Trace_Key");
}

TEST(NetworkingDiagnostic, SerializerReusesDiagnosticPrefix)
{
	auto diagnostic = Networking::Diagnostic::Builder(
		Networking::Diagnostic::Severity::Warning,
		"Shared_Prefix",
		5u)
		.build();

	spk::Message destination(99u);
	destination << diagnostic;

	EXPECT_EQ(
		destination.readAt<std::uint8_t>(0u),
		static_cast<std::uint8_t>(
			Networking::Diagnostic::Severity::Warning));
	EXPECT_EQ(
		destination.readAt<std::uint32_t>(sizeof(std::uint8_t)),
		std::string("Shared_Prefix").size());
}

TEST(NetworkingDiagnostic, RejectsWrongMessageType)
{
	auto source = Networking::Diagnostic::Builder(
		Networking::Diagnostic::Severity::Info,
		"Key")
		.build();
	spk::Message raw = source;
	raw.setType(
		static_cast<spk::Message::Type>(
			Networking::MessageType::ChunkError));

	EXPECT_THROW(
		(void)Networking::Diagnostic(raw),
		spk::Exception);
}

TEST(NetworkingDiagnostic, RejectsUnknownSeverity)
{
	spk::Message raw(diagnosticMessageType());
	raw << std::uint8_t{99u};
	raw << std::string("Key");

	EXPECT_THROW(
		(void)Networking::Diagnostic(raw),
		spk::Exception);
}

TEST(NetworkingDiagnostic, RejectsTruncatedString)
{
	spk::Message raw(diagnosticMessageType());
	raw << static_cast<std::uint8_t>(
		Networking::Diagnostic::Severity::Error);
	raw << std::uint32_t{5u};
	raw.append("abc", 3u);

	EXPECT_THROW(
		(void)Networking::Diagnostic(raw),
		spk::Exception);
}

TEST(NetworkingDiagnostic, RejectsTrailingBytes)
{
	auto source = Networking::Diagnostic::Builder(
		Networking::Diagnostic::Severity::Error,
		"Key")
		.build();
	spk::Message raw = source;
	raw << std::uint8_t{0u};

	EXPECT_THROW(
		(void)Networking::Diagnostic(raw),
		spk::Exception);
}
