#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/chunk_protocol.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <vector>

namespace
{
	constexpr std::size_t SummarySize = 3u * sizeof(std::uint32_t);
	constexpr std::size_t CellCount =
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent);
	constexpr std::size_t CoordinateAndStateSize =
		sizeof(Chunk::Coordinate) + sizeof(std::uint8_t);
	constexpr std::size_t SuccessEntrySize =
		CoordinateAndStateSize + CellCount * sizeof(Voxel::Cell::PackedType);
	constexpr std::size_t ResultEntrySize = CoordinateAndStateSize;

	[[nodiscard]] spk::Message::Type responseMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(Networking::MessageType::ChunkResponse);
	}

	Chunk makeSequentialChunk()
	{
		Chunk::Builder builder;
		Voxel::Cell::PackedType packed = 1u;

		for (std::int32_t z = 0; z < Chunk::Extent; ++z)
		{
			for (std::int32_t x = 0; x < Chunk::Extent; ++x)
			{
				for (std::int32_t y = 0; y < Chunk::Extent; ++y)
				{
					EXPECT_TRUE(builder.set({x, y, z}, Voxel::Cell(packed)));
					++packed;
				}
			}
		}

		return std::move(builder).build();
	}

	Chunk makeEmptyChunk()
	{
		return std::move(Chunk::Builder{}).build();
	}

	spk::Message responseMessage(spk::Message::RequestID requestID)
	{
		spk::Message message(responseMessageType());
		message.setRequestID(requestID);
		return message;
	}

	void appendSummary(
		spk::Message &message,
		std::uint32_t successOffset,
		std::uint32_t rejectedOffset,
		std::uint32_t unavailableOffset)
	{
		message.append(successOffset);
		message.append(rejectedOffset);
		message.append(unavailableOffset);
	}

	void appendResult(
		spk::Message &message,
		const Chunk::Coordinate &coordinate,
		Chunk::Protocol::Response::State state)
	{
		message.append(coordinate);
		message.append(static_cast<std::uint8_t>(state));
	}

	void expectPayloadsEqual(
		const spk::Message &first,
		const spk::Message &second)
	{
		ASSERT_EQ(first.size(), second.size());
		EXPECT_TRUE(std::equal(
			first.data().begin(),
			first.data().end(),
			second.data().begin(),
			second.data().end()));
	}
}

TEST(ChunkProtocolResponse, ConstructionWritesExactlyTheThreeOffsetSummary)
{
	const Chunk::Protocol::Response response(100u);

	EXPECT_EQ(response.type(), responseMessageType());
	EXPECT_EQ(response.requestID(), 100u);
	EXPECT_EQ(response.size(), SummarySize);
	EXPECT_EQ(response.successOffset(), SummarySize);
	EXPECT_EQ(response.rejectedOffset(), SummarySize);
	EXPECT_EQ(response.unavailableOffset(), SummarySize);
	EXPECT_EQ(response.readAt<std::uint32_t>(0u), SummarySize);
	EXPECT_EQ(response.readAt<std::uint32_t>(sizeof(std::uint32_t)), SummarySize);
	EXPECT_EQ(
		response.readAt<std::uint32_t>(2u * sizeof(std::uint32_t)),
		SummarySize);
}

TEST(ChunkProtocolResponse, RejectsZeroOriginatingRequestID)
{
	EXPECT_THROW((void)Chunk::Protocol::Response(0u), spk::Exception);
}

TEST(ChunkProtocolResponse, SuccessOnlyUsesFixedChunkEntryAndCellOrder)
{
	const Chunk chunk = makeSequentialChunk();
	const Chunk::Coordinate coordinate{-3, 8, 11};

	Chunk::Protocol::Response response(101u);
	response.addSuccess(coordinate, chunk);

	EXPECT_EQ(response.successOffset(), SummarySize);
	EXPECT_EQ(response.rejectedOffset(), SummarySize + SuccessEntrySize);
	EXPECT_EQ(response.unavailableOffset(), SummarySize + SuccessEntrySize);
	EXPECT_EQ(response.size(), SummarySize + SuccessEntrySize);
	EXPECT_EQ(
		response.readAt<Chunk::Coordinate>(response.successOffset()),
		coordinate);
	EXPECT_EQ(
		response.readAt<std::uint8_t>(
			response.successOffset() + sizeof(Chunk::Coordinate)),
		static_cast<std::uint8_t>(Chunk::Protocol::Response::State::Success));

	const std::size_t cellOffset =
		response.successOffset() + CoordinateAndStateSize;
	for (std::size_t index = 0; index < CellCount; ++index)
	{
		EXPECT_EQ(
			response.readAt<Voxel::Cell::PackedType>(
				cellOffset + index * sizeof(Voxel::Cell::PackedType)),
			index + 1u);
	}
}

TEST(ChunkProtocolResponse, RejectedOnlyUsesEqualEmptySuccessBoundary)
{
	Chunk::Protocol::Response response(102u);
	response.addRejected({1, 2, 3});

	EXPECT_EQ(response.successOffset(), SummarySize);
	EXPECT_EQ(response.rejectedOffset(), SummarySize);
	EXPECT_EQ(response.unavailableOffset(), SummarySize + ResultEntrySize);
	EXPECT_EQ(response.size(), SummarySize + ResultEntrySize);
	EXPECT_EQ(
		response.readAt<std::uint8_t>(
			response.rejectedOffset() + sizeof(Chunk::Coordinate)),
		static_cast<std::uint8_t>(Chunk::Protocol::Response::State::Rejected));
}

TEST(ChunkProtocolResponse, UnavailableOnlyUsesEqualSuccessAndRejectedBoundaries)
{
	Chunk::Protocol::Response response(103u);
	response.addUnavailable({1, 2, 3});

	EXPECT_EQ(response.successOffset(), SummarySize);
	EXPECT_EQ(response.rejectedOffset(), SummarySize);
	EXPECT_EQ(response.unavailableOffset(), SummarySize);
	EXPECT_EQ(response.size(), SummarySize + ResultEntrySize);
	EXPECT_EQ(
		response.readAt<std::uint8_t>(
			response.unavailableOffset() + sizeof(Chunk::Coordinate)),
		static_cast<std::uint8_t>(Chunk::Protocol::Response::State::Unavailable));
}

TEST(ChunkProtocolResponse, EncodesGroupsAndCoordinatesCanonicallyRegardlessOfAddOrder)
{
	const Chunk chunk = makeEmptyChunk();

	Chunk::Protocol::Response response(104u);
	response.addUnavailable({9, 0, 0});
	response.addRejected({3, 0, 0});
	response.addSuccess({8, 0, 0}, chunk);
	response.addRejected({-4, 5, 0});
	response.addSuccess({-2, 7, 1}, chunk);
	response.addUnavailable({0, 0, -1});

	ASSERT_EQ(
		response.rejectedOffset(),
		SummarySize + 2u * SuccessEntrySize);
	ASSERT_EQ(
		response.unavailableOffset(),
		response.rejectedOffset() + 2u * ResultEntrySize);

	EXPECT_EQ(
		response.readAt<Chunk::Coordinate>(response.successOffset()),
		(Chunk::Coordinate{-2, 7, 1}));
	EXPECT_EQ(
		response.readAt<Chunk::Coordinate>(
			response.successOffset() + SuccessEntrySize),
		(Chunk::Coordinate{8, 0, 0}));

	EXPECT_EQ(
		response.readAt<Chunk::Coordinate>(response.rejectedOffset()),
		(Chunk::Coordinate{-4, 5, 0}));
	EXPECT_EQ(
		response.readAt<Chunk::Coordinate>(
			response.rejectedOffset() + ResultEntrySize),
		(Chunk::Coordinate{3, 0, 0}));

	EXPECT_EQ(
		response.readAt<Chunk::Coordinate>(response.unavailableOffset()),
		(Chunk::Coordinate{0, 0, -1}));
	EXPECT_EQ(
		response.readAt<Chunk::Coordinate>(
			response.unavailableOffset() + ResultEntrySize),
		(Chunk::Coordinate{9, 0, 0}));
}

TEST(ChunkProtocolResponse, SameLogicalResultsProduceIdenticalPayloadBytes)
{
	const Chunk chunk = makeEmptyChunk();

	Chunk::Protocol::Response first(105u);
	first.addUnavailable({7, 0, 0});
	first.addSuccess({4, 0, 0}, chunk);
	first.addRejected({-3, 0, 0});
	first.addSuccess({-5, 0, 0}, chunk);

	Chunk::Protocol::Response second(105u);
	second.addSuccess({-5, 0, 0}, chunk);
	second.addRejected({-3, 0, 0});
	second.addSuccess({4, 0, 0}, chunk);
	second.addUnavailable({7, 0, 0});

	expectPayloadsEqual(first, second);
}

TEST(ChunkProtocolResponse, RoundTripPreservesValidatedRangesAndAllowsFurtherAdds)
{
	Chunk::Protocol::Response source(106u);
	source.addRejected({-5, 4, 3});

	const spk::Message raw = source;
	Chunk::Protocol::Response decoded(raw);

	EXPECT_EQ(decoded.successOffset(), source.successOffset());
	EXPECT_EQ(decoded.rejectedOffset(), source.rejectedOffset());
	EXPECT_EQ(decoded.unavailableOffset(), source.unavailableOffset());

	decoded.addUnavailable({8, 9, 10});

	EXPECT_EQ(
		decoded.readAt<Chunk::Coordinate>(decoded.rejectedOffset()),
		(Chunk::Coordinate{-5, 4, 3}));
	EXPECT_EQ(
		decoded.readAt<Chunk::Coordinate>(decoded.unavailableOffset()),
		(Chunk::Coordinate{8, 9, 10}));
}

TEST(ChunkProtocolResponse, ValidatedRangesSupportReadAtWithoutChangingCursor)
{
	Chunk::Protocol::Response source(107u);
	source.addRejected({-7, 2, 9});

	spk::Message raw = source;
	raw.skip<std::uint32_t>();
	const Chunk::Protocol::Response decoded(raw);
	const auto originalReadOffset = decoded.readOffset();

	EXPECT_EQ(
		decoded.readAt<Chunk::Coordinate>(decoded.rejectedOffset()),
		(Chunk::Coordinate{-7, 2, 9}));
	EXPECT_EQ(decoded.readOffset(), originalReadOffset);
	EXPECT_EQ(raw.readOffset(), sizeof(std::uint32_t));
}

TEST(ChunkProtocolResponse, DecodedPayloadOutlivesSourceMessage)
{
	const Chunk::Protocol::Response decoded = [] {
		Chunk::Protocol::Response source(108u);
		source.addUnavailable({1, -2, 3});
		const spk::Message raw = source;
		return Chunk::Protocol::Response(raw);
	}();

	EXPECT_EQ(
		decoded.readAt<Chunk::Coordinate>(decoded.unavailableOffset()),
		(Chunk::Coordinate{1, -2, 3}));
}

TEST(ChunkProtocolResponse, AddRejectsDuplicateCoordinateAcrossStates)
{
	const Chunk chunk = makeEmptyChunk();
	Chunk::Protocol::Response response(109u);
	response.addSuccess({1, 2, 3}, chunk);

	const auto previousSize = response.size();
	EXPECT_THROW(response.addRejected({1, 2, 3}), spk::Exception);
	EXPECT_THROW(response.addUnavailable({1, 2, 3}), spk::Exception);
	EXPECT_EQ(response.size(), previousSize);
}

TEST(ChunkProtocolResponse, RejectsWrongMessageType)
{
	Chunk::Protocol::Response valid(110u);
	spk::Message raw = valid;
	raw.setType(
		static_cast<spk::Message::Type>(Networking::MessageType::ChunkRequest));

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsZeroRequestID)
{
	Chunk::Protocol::Response valid(111u);
	spk::Message raw = valid;
	raw.setRequestID(0u);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsTruncatedSummary)
{
	spk::Message raw = responseMessage(112u);
	raw.append(std::uint32_t{12u});
	raw.append(std::uint32_t{12u});

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsSuccessOffsetDifferentFromTwelve)
{
	spk::Message raw = responseMessage(113u);
	appendSummary(raw, 11u, 12u, 12u);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsOffsetsOutOfOrder)
{
	spk::Message raw = responseMessage(114u);
	appendSummary(raw, 12u, 25u, 12u);
	raw.resize(25u);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsOffsetsOutsidePayloadBounds)
{
	spk::Message raw = responseMessage(115u);
	appendSummary(raw, 12u, 12u, 100u);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsMisalignedSuccessRange)
{
	spk::Message raw = responseMessage(116u);
	appendSummary(raw, 12u, 13u, 13u);
	raw.resize(13u);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsMisalignedRejectedRange)
{
	spk::Message raw = responseMessage(117u);
	appendSummary(raw, 12u, 12u, 13u);
	raw.resize(13u);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsMisalignedUnavailableRangeAndTrailingBytes)
{
	Chunk::Protocol::Response valid(118u);
	valid.addRejected({1, 2, 3});

	spk::Message raw = valid;
	raw.append(std::uint8_t{0u});

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsKnownStateInWrongGroup)
{
	Chunk::Protocol::Response valid(119u);
	valid.addRejected({1, 2, 3});

	spk::Message raw = valid;
	const std::uint8_t wrongState =
		static_cast<std::uint8_t>(Chunk::Protocol::Response::State::Unavailable);
	raw.edit(
		valid.rejectedOffset() + sizeof(Chunk::Coordinate),
		wrongState);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsUnknownState)
{
	Chunk::Protocol::Response valid(120u);
	valid.addRejected({1, 2, 3});

	spk::Message raw = valid;
	const std::uint8_t unknownState = 99u;
	raw.edit(
		valid.rejectedOffset() + sizeof(Chunk::Coordinate),
		unknownState);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsDuplicateCoordinateAcrossGroups)
{
	spk::Message raw = responseMessage(121u);
	appendSummary(
		raw,
		SummarySize,
		SummarySize,
		SummarySize + ResultEntrySize);
	appendResult(
		raw,
		{1, 2, 3},
		Chunk::Protocol::Response::State::Rejected);
	appendResult(
		raw,
		{1, 2, 3},
		Chunk::Protocol::Response::State::Unavailable);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsUnsortedCoordinatesInsideAGroup)
{
	spk::Message raw = responseMessage(122u);
	appendSummary(
		raw,
		SummarySize,
		SummarySize,
		SummarySize + 2u * ResultEntrySize);
	appendResult(
		raw,
		{2, 0, 0},
		Chunk::Protocol::Response::State::Rejected);
	appendResult(
		raw,
		{1, 0, 0},
		Chunk::Protocol::Response::State::Rejected);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsTruncatedSuccessChunkCellBlock)
{
	spk::Message raw = responseMessage(123u);
	const auto truncatedEnd =
		static_cast<std::uint32_t>(SummarySize + SuccessEntrySize - 1u);
	appendSummary(raw, SummarySize, truncatedEnd, truncatedEnd);
	raw.resize(truncatedEnd);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsDuplicateCoordinatesWithinAGroup)
{
	spk::Message raw = responseMessage(124u);
	appendSummary(
		raw,
		SummarySize,
		SummarySize,
		SummarySize + 2u * ResultEntrySize);
	appendResult(
		raw,
		{1, 0, 0},
		Chunk::Protocol::Response::State::Rejected);
	appendResult(
		raw,
		{1, 0, 0},
		Chunk::Protocol::Response::State::Rejected);

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}
