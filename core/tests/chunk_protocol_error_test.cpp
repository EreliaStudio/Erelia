#include "erelia/core/chunk_protocol_error.hpp"
#include "erelia/core/networking/message_type.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <cstddef>
#include <cstdint>
#include <utility>

namespace
{
	constexpr std::size_t EntrySize =
		sizeof(std::uint8_t) + sizeof(Chunk::Coordinate);

	[[nodiscard]] spk::Message::Type errorMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(Networking::MessageType::ChunkError);
	}

	void appendEntry(
		spk::Message &message,
		std::uint8_t code,
		const Chunk::Coordinate &coordinate)
	{
		message.append(code);
		message.append(coordinate);
	}

	spk::Message errorMessage(spk::Message::RequestID requestID)
	{
		spk::Message message(errorMessageType());
		message.setRequestID(requestID);
		return message;
	}
}

TEST(ChunkProtocolError, EmptyBuilderProducesTypedCorrelatedMessage)
{
	Chunk::Protocol::Error::Builder builder(91u);
	const auto error = std::move(builder).build();

	EXPECT_EQ(error.type(), errorMessageType());
	EXPECT_EQ(error.requestID(), 91u);
	EXPECT_TRUE(error.empty());
	EXPECT_EQ(error.entryCount(), 0u);
}

TEST(ChunkProtocolError, BuilderRejectsZeroOriginatingRequestID)
{
	EXPECT_THROW(
		(void)Chunk::Protocol::Error::Builder(0u),
		spk::Exception);
}

TEST(ChunkProtocolError, BuilderSortsEntriesAndSerializesNoCount)
{
	const Chunk::Coordinate high{5, 0, 0};
	const Chunk::Coordinate low{-2, 9, 9};
	const Chunk::Coordinate middle{5, -1, 7};

	Chunk::Protocol::Error::Builder builder(12u);
	builder.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		high);
	builder.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		low);
	builder.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		middle);

	const auto error = std::move(builder).build();

	ASSERT_EQ(error.size(), 3u * EntrySize);
	ASSERT_EQ(error.entryCount(), 3u);
	EXPECT_EQ(error.entry(0u).coordinate, low);
	EXPECT_EQ(error.entry(1u).coordinate, middle);
	EXPECT_EQ(error.entry(2u).coordinate, high);

	for (std::size_t index = 0; index < error.entryCount(); ++index)
	{
		const std::size_t offset = index * EntrySize;
		EXPECT_EQ(
			error.readAt<std::uint8_t>(offset),
			static_cast<std::uint8_t>(
				Chunk::Protocol::Error::Code::DuplicateCoordinate));
		EXPECT_EQ(
			error.readAt<Chunk::Coordinate>(offset + sizeof(std::uint8_t)),
			error.entry(index).coordinate);
	}
}

TEST(ChunkProtocolError, PreservesNegativeCoordinatesExactly)
{
	const Chunk::Coordinate coordinate{-100, -200, -300};

	Chunk::Protocol::Error::Builder builder(13u);
	builder.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		coordinate);
	const auto error = std::move(builder).build();

	EXPECT_EQ(
		error.readAt<Chunk::Coordinate>(sizeof(std::uint8_t)),
		coordinate);
	ASSERT_EQ(error.entryCount(), 1u);
	EXPECT_EQ(error.entry(0u).coordinate, coordinate);
}

TEST(ChunkProtocolError, RoundTripsAndOwnsDecodedPayload)
{
	Chunk::Protocol::Error::Builder builder(14u);
	builder.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		{3, 2, 1});
	builder.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		{-1, 7, 4});
	const auto source = std::move(builder).build();

	const spk::Message raw = source;
	const Chunk::Protocol::Error decoded(raw);

	EXPECT_EQ(decoded.requestID(), source.requestID());
	EXPECT_EQ(decoded.entryCount(), source.entryCount());
	EXPECT_EQ(decoded.entry(0u), source.entry(0u));
	EXPECT_EQ(decoded.entry(1u), source.entry(1u));
	EXPECT_EQ(decoded.size(), source.size());
}

TEST(ChunkProtocolError, DecodingDoesNotMoveTheSourceCursor)
{
	Chunk::Protocol::Error::Builder builder(15u);
	builder.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		{1, 2, 3});
	const auto source = std::move(builder).build();

	spk::Message raw = source;
	raw.skip<std::uint8_t>();
	const auto originalReadOffset = raw.readOffset();

	const Chunk::Protocol::Error decoded(raw);

	EXPECT_EQ(raw.readOffset(), originalReadOffset);
	EXPECT_EQ(decoded.readOffset(), originalReadOffset);
}

TEST(ChunkProtocolError, AccessorReadsTheMessagePayloadAsItsSourceOfTruth)
{
	Chunk::Protocol::Error::Builder builder(16u);
	builder.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		{1, 2, 3});
	auto error = std::move(builder).build();

	const Chunk::Coordinate replacement{-4, 5, 6};
	error.edit(sizeof(std::uint8_t), replacement);

	EXPECT_EQ(error.entry(0u).coordinate, replacement);
}

TEST(ChunkProtocolError, EntryRejectsOutOfRangeIndex)
{
	Chunk::Protocol::Error::Builder builder(17u);
	const auto error = std::move(builder).build();

	EXPECT_THROW((void)error.entry(0u), spk::Exception);
}

TEST(ChunkProtocolError, RejectsWrongMessageType)
{
	spk::Message raw(
		static_cast<spk::Message::Type>(Networking::MessageType::ChunkResponse));
	raw.setRequestID(1u);

	EXPECT_THROW((void)Chunk::Protocol::Error(raw), spk::Exception);
}

TEST(ChunkProtocolError, RejectsZeroRequestID)
{
	spk::Message raw(errorMessageType());

	EXPECT_THROW((void)Chunk::Protocol::Error(raw), spk::Exception);
}

TEST(ChunkProtocolError, RejectsTruncatedOrMisalignedEntry)
{
	spk::Message raw = errorMessage(2u);
	const std::uint8_t code = 0u;
	raw.append(code);
	const Chunk::Coordinate coordinate{1, 2, 3};
	raw.append(&coordinate, sizeof(coordinate) - 1u);

	EXPECT_THROW((void)Chunk::Protocol::Error(raw), spk::Exception);
}

TEST(ChunkProtocolError, RejectsUnknownCode)
{
	spk::Message raw = errorMessage(3u);
	appendEntry(raw, 99u, {1, 2, 3});

	EXPECT_THROW((void)Chunk::Protocol::Error(raw), spk::Exception);
}

TEST(ChunkProtocolError, RejectsDuplicateCoordinates)
{
	spk::Message raw = errorMessage(4u);
	appendEntry(raw, 0u, {1, 2, 3});
	appendEntry(raw, 0u, {1, 2, 3});

	EXPECT_THROW((void)Chunk::Protocol::Error(raw), spk::Exception);
}

TEST(ChunkProtocolError, RejectsUnsortedCoordinates)
{
	spk::Message raw = errorMessage(5u);
	appendEntry(raw, 0u, {2, 0, 0});
	appendEntry(raw, 0u, {1, 0, 0});

	EXPECT_THROW((void)Chunk::Protocol::Error(raw), spk::Exception);
}

TEST(ChunkProtocolError, BuilderRejectsDuplicateCoordinate)
{
	Chunk::Protocol::Error::Builder builder(6u);
	builder.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		{1, 2, 3});

	EXPECT_THROW(
		builder.add(
			Chunk::Protocol::Error::Code::DuplicateCoordinate,
			{1, 2, 3}),
		spk::Exception);
}

TEST(ChunkProtocolError, BuilderRejectsUnknownCode)
{
	Chunk::Protocol::Error::Builder builder(7u);

	EXPECT_THROW(
		builder.add(
			static_cast<Chunk::Protocol::Error::Code>(99u),
			{1, 2, 3}),
		spk::Exception);
}
