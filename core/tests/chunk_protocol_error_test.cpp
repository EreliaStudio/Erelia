#include "erelia/core/chunk_protocol_error.hpp"
#include "erelia/core/networking/message_type.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <cstddef>
#include <cstdint>
#include <string>
#include <utility>

namespace
{
	constexpr std::string_view DuplicateKey =
		"Chunk_Coordinates_Duplication";
	constexpr std::size_t DiagnosticPrefixSize =
		sizeof(std::uint8_t) +
		sizeof(std::uint32_t) +
		DuplicateKey.size();

	[[nodiscard]] spk::Message::Type errorMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(
			Networking::MessageType::ChunkError);
	}

	Chunk::Protocol::Error buildError(
		spk::Message::RequestID requestID = 91u)
	{
		Chunk::Protocol::Error::Builder builder(
			requestID,
			Networking::Diagnostic::Severity::Warning,
			std::string(DuplicateKey));
		return std::move(builder).build();
	}
}

TEST(ChunkProtocolError, EmptyBuilderProducesCorrelatedDiagnostic)
{
	const auto error = buildError();

	EXPECT_EQ(error.type(), errorMessageType());
	EXPECT_EQ(error.requestID(), 91u);
	EXPECT_EQ(
		error.severity(),
		Networking::Diagnostic::Severity::Warning);
	EXPECT_EQ(error.message(), DuplicateKey);
	EXPECT_EQ(error.coordinateCount(), 0u);
	EXPECT_EQ(
		error.size(),
		DiagnosticPrefixSize + sizeof(std::uint32_t));
}

TEST(ChunkProtocolError, BuilderRejectsZeroOriginatingRequestID)
{
	EXPECT_THROW(
		(void)Chunk::Protocol::Error::Builder(
			0u,
			Networking::Diagnostic::Severity::Warning,
			std::string(DuplicateKey)),
		spk::Exception);
}

TEST(ChunkProtocolError, BuilderSortsCoordinatesAndSerializesUint32Count)
{
	const Chunk::Coordinate high{5, 0, 0};
	const Chunk::Coordinate low{-2, 9, 9};
	const Chunk::Coordinate middle{5, -1, 7};

	Chunk::Protocol::Error::Builder builder(
		12u,
		Networking::Diagnostic::Severity::Warning,
		std::string(DuplicateKey));
	builder.add(high);
	builder.add(low);
	builder.add(middle);

	const auto error = std::move(builder).build();

	ASSERT_EQ(error.coordinateCount(), 3u);
	EXPECT_EQ(
		error.reader().readAt<std::uint32_t>(DiagnosticPrefixSize),
		3u);
	EXPECT_EQ(error.coordinate(0u), low);
	EXPECT_EQ(error.coordinate(1u), middle);
	EXPECT_EQ(error.coordinate(2u), high);
}

TEST(ChunkProtocolError, PreservesNegativeCoordinatesExactly)
{
	const Chunk::Coordinate coordinate{-100, -200, -300};

	Chunk::Protocol::Error::Builder builder(
		13u,
		Networking::Diagnostic::Severity::Warning,
		std::string(DuplicateKey));
	builder.add(coordinate);
	const auto error = std::move(builder).build();

	ASSERT_EQ(error.coordinateCount(), 1u);
	EXPECT_EQ(error.coordinate(0u), coordinate);
}

TEST(ChunkProtocolError, RoundTripsAndOwnsDecodedPayload)
{
	Chunk::Protocol::Error::Builder builder(
		14u,
		Networking::Diagnostic::Severity::Warning,
		std::string(DuplicateKey));
	builder.add({3, 2, 1});
	builder.add({-1, 7, 4});
	const auto source = std::move(builder).build();

	const spk::Message raw = source;
	const Chunk::Protocol::Error decoded(raw);

	EXPECT_EQ(decoded.requestID(), source.requestID());
	EXPECT_EQ(decoded.severity(), source.severity());
	EXPECT_EQ(decoded.message(), source.message());
	ASSERT_EQ(decoded.coordinateCount(), source.coordinateCount());
	EXPECT_EQ(decoded.coordinate(0u), source.coordinate(0u));
	EXPECT_EQ(decoded.coordinate(1u), source.coordinate(1u));
}

TEST(ChunkProtocolError, DecodingDoesNotMoveExternalReader)
{
	const auto source = buildError(15u);
	const spk::Message raw = source;
	auto externalReader = raw.reader();
	externalReader.skip<std::uint8_t>();
	const auto originalReadOffset =
		externalReader.readOffset();

	const Chunk::Protocol::Error decoded(raw);

	EXPECT_EQ(
		externalReader.readOffset(),
		originalReadOffset);
	EXPECT_EQ(decoded.requestID(), 15u);
}

TEST(ChunkProtocolError, CoordinateRejectsOutOfRangeIndex)
{
	const auto error = buildError(17u);

	EXPECT_THROW(
		(void)error.coordinate(0u),
		spk::Exception);
}

TEST(ChunkProtocolError, RejectsWrongMessageType)
{
	spk::Message::Writer writer(
		static_cast<spk::Message::Type>(
			Networking::MessageType::Diagnostic));
	writer.setRequestID(1u);
	writer << static_cast<std::uint8_t>(
		Networking::Diagnostic::Severity::Warning);
	writer << std::string(DuplicateKey);
	writer << std::uint32_t{0u};
	const spk::Message raw = std::move(writer).build();

	EXPECT_THROW(
		(void)Chunk::Protocol::Error(raw),
		spk::Exception);
}

TEST(ChunkProtocolError, RejectsZeroRequestID)
{
	spk::Message raw = buildError(2u);
	spk::Message::Writer writer(std::move(raw));
	writer.setRequestID(0u);
	raw = std::move(writer).build();

	EXPECT_THROW(
		(void)Chunk::Protocol::Error(raw),
		spk::Exception);
}

TEST(ChunkProtocolError, RejectsUnknownSeverity)
{
	spk::Message::Writer writer(errorMessageType());
	writer.setRequestID(3u);
	writer << std::uint8_t{99u};
	writer << std::string(DuplicateKey);
	writer << std::uint32_t{0u};
	const spk::Message raw = std::move(writer).build();

	EXPECT_THROW(
		(void)Chunk::Protocol::Error(raw),
		spk::Exception);
}

TEST(ChunkProtocolError, RejectsMissingCoordinateCount)
{
	const auto valid = buildError(4u);
	spk::Message raw = valid;
	spk::Message::Writer writer(std::move(raw));
	writer.resize(DiagnosticPrefixSize);
	raw = std::move(writer).build();

	EXPECT_THROW(
		(void)Chunk::Protocol::Error(raw),
		spk::Exception);
}

TEST(ChunkProtocolError, RejectsCoordinateCountMismatch)
{
	Chunk::Protocol::Error::Builder builder(
		5u,
		Networking::Diagnostic::Severity::Warning,
		std::string(DuplicateKey));
	builder.add({1, 2, 3});
	auto valid = std::move(builder).build();

	spk::Message raw = valid;
	spk::Message::Writer writer(std::move(raw));
	writer.edit(
		DiagnosticPrefixSize,
		std::uint32_t{2u});
	raw = std::move(writer).build();

	EXPECT_THROW(
		(void)Chunk::Protocol::Error(raw),
		spk::Exception);
}

TEST(ChunkProtocolError, RejectsDuplicateCoordinates)
{
	Chunk::Protocol::Error::Builder builder(
		6u,
		Networking::Diagnostic::Severity::Warning,
		std::string(DuplicateKey));
	builder.add({1, 2, 3});
	auto valid = std::move(builder).build();

	spk::Message raw = valid;
	spk::Message::Writer writer(std::move(raw));
	writer.edit(
		DiagnosticPrefixSize,
		std::uint32_t{2u});
	writer.append(
		valid.coordinate(0u));
	raw = std::move(writer).build();

	EXPECT_THROW(
		(void)Chunk::Protocol::Error(raw),
		spk::Exception);
}

TEST(ChunkProtocolError, RejectsUnsortedCoordinates)
{
	Chunk::Protocol::Error::Builder builder(
		7u,
		Networking::Diagnostic::Severity::Warning,
		std::string(DuplicateKey));
	builder.add({1, 0, 0});
	builder.add({2, 0, 0});
	auto valid = std::move(builder).build();

	spk::Message raw = valid;
	spk::Message::Writer writer(std::move(raw));
	const std::size_t firstCoordinateOffset =
		DiagnosticPrefixSize +
		sizeof(std::uint32_t);
	writer.edit(
		firstCoordinateOffset,
		Chunk::Coordinate{2, 0, 0});
	writer.edit(
		firstCoordinateOffset + sizeof(Chunk::Coordinate),
		Chunk::Coordinate{1, 0, 0});
	raw = std::move(writer).build();

	EXPECT_THROW(
		(void)Chunk::Protocol::Error(raw),
		spk::Exception);
}

TEST(ChunkProtocolError, BuilderRejectsDuplicateCoordinate)
{
	Chunk::Protocol::Error::Builder builder(
		8u,
		Networking::Diagnostic::Severity::Warning,
		std::string(DuplicateKey));
	builder.add({1, 2, 3});

	EXPECT_THROW(
		builder.add({1, 2, 3}),
		spk::Exception);
}

TEST(ChunkProtocolError, BuilderRejectsUnknownSeverityAtBuild)
{
	Chunk::Protocol::Error::Builder builder(
		9u,
		static_cast<Networking::Diagnostic::Severity>(99u),
		std::string(DuplicateKey));

	EXPECT_THROW(
		(void)std::move(builder).build(),
		spk::Exception);
}
