#include "erelia/core/chunk_protocol.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <cstddef>
#include <cstdint>
#include <vector>

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

TEST(ChunkProtocolError, ConstructionOwnsTypeAndOriginatingRequestID)
{
	const Chunk::Protocol::Error error(91u);

	EXPECT_EQ(error.type(), errorMessageType());
	EXPECT_EQ(error.requestID(), 91u);
	EXPECT_TRUE(error.empty());
	EXPECT_TRUE(error.entries().empty());
}

TEST(ChunkProtocolError, RejectsZeroOriginatingRequestID)
{
	EXPECT_THROW((void)Chunk::Protocol::Error(0u), spk::Exception);
}

TEST(ChunkProtocolError, AddSortsEntriesAndSerializesNoCount)
{
	const Chunk::Coordinate high{5, 0, 0};
	const Chunk::Coordinate low{-2, 9, 9};
	const Chunk::Coordinate middle{5, -1, 7};

	Chunk::Protocol::Error error(12u);
	error.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		high);
	error.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		low);
	error.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		middle);

	ASSERT_EQ(error.size(), 3u * EntrySize);
	ASSERT_EQ(error.entries().size(), 3u);
	EXPECT_EQ(error.entries()[0].coordinate, low);
	EXPECT_EQ(error.entries()[1].coordinate, middle);
	EXPECT_EQ(error.entries()[2].coordinate, high);

	for (std::size_t index = 0; index < error.entries().size(); ++index)
	{
		const std::size_t offset = index * EntrySize;
		EXPECT_EQ(
			error.readAt<std::uint8_t>(offset),
			static_cast<std::uint8_t>(
				Chunk::Protocol::Error::Code::DuplicateCoordinate));
		EXPECT_EQ(
			error.readAt<Chunk::Coordinate>(offset + sizeof(std::uint8_t)),
			error.entries()[index].coordinate);
	}
}

TEST(ChunkProtocolError, PreservesNegativeCoordinatesExactly)
{
	const Chunk::Coordinate coordinate{-100, -200, -300};

	Chunk::Protocol::Error error(13u);
	error.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		coordinate);

	EXPECT_EQ(
		error.readAt<Chunk::Coordinate>(sizeof(std::uint8_t)),
		coordinate);
	ASSERT_EQ(error.entries().size(), 1u);
	EXPECT_EQ(error.entries()[0].coordinate, coordinate);
}

TEST(ChunkProtocolError, RoundTripsAndOwnsDecodedEntries)
{
	Chunk::Protocol::Error source(14u);
	source.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		{3, 2, 1});
	source.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		{-1, 7, 4});

	const spk::Message raw = source;
	const Chunk::Protocol::Error decoded(raw);

	EXPECT_EQ(decoded.requestID(), source.requestID());
	EXPECT_EQ(decoded.entries(), source.entries());
	EXPECT_EQ(decoded.size(), source.size());
}

TEST(ChunkProtocolError, DecodingDoesNotMoveTheSourceCursor)
{
	Chunk::Protocol::Error source(15u);
	source.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		{1, 2, 3});

	spk::Message raw = source;
	raw.skip<std::uint8_t>();
	const auto originalReadOffset = raw.readOffset();

	const Chunk::Protocol::Error decoded(raw);

	EXPECT_EQ(raw.readOffset(), originalReadOffset);
	EXPECT_EQ(decoded.readOffset(), originalReadOffset);
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

TEST(ChunkProtocolError, AddRejectsDuplicateWithoutMutatingEncodedMessage)
{
	Chunk::Protocol::Error error(6u);
	error.add(
		Chunk::Protocol::Error::Code::DuplicateCoordinate,
		{1, 2, 3});

	const auto previousSize = error.size();
	const auto previousEntries = error.entries();

	EXPECT_THROW(
		error.add(
			Chunk::Protocol::Error::Code::DuplicateCoordinate,
			{1, 2, 3}),
		spk::Exception);

	EXPECT_EQ(error.size(), previousSize);
	EXPECT_EQ(error.entries(), previousEntries);
}

TEST(ChunkProtocolError, AddRejectsUnknownCode)
{
	Chunk::Protocol::Error error(7u);

	EXPECT_THROW(
		error.add(
			static_cast<Chunk::Protocol::Error::Code>(99u),
			{1, 2, 3}),
		spk::Exception);
	EXPECT_TRUE(error.empty());
	EXPECT_TRUE(error.entries().empty());
}
