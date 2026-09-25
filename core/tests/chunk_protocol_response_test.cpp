#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/chunk_protocol_response.hpp"\n#include "erelia/core/networking/message_type.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <utility>

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
		CoordinateAndStateSize + CellCount * sizeof(Voxel::Cell);
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

	Chunk::Protocol::Response emptyResponse(spk::Message::RequestID requestID)
	{
		Chunk::Protocol::Response::Builder builder(requestID);
		return std::move(builder).build();
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
		EXPECT_TRUE(
			std::equal(
				first.data().begin(),
				first.data().end(),
				second.data().begin(),
				second.data().end()));
	}
}

TEST(ChunkProtocolResponse, EmptyBuilderWritesExactlyTheThreeOffsetSummary)
{
	const auto response = emptyResponse(100u);

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

TEST(ChunkProtocolResponse, BuilderRejectsZeroOriginatingRequestID)
{
	EXPECT_THROW(
		(void)Chunk::Protocol::Response::Builder(0u),
		spk::Exception);
}

TEST(ChunkProtocolResponse, SuccessOnlyUsesFixedChunkEntryAndCellOrder)
{
	const Chunk chunk = makeSequentialChunk();
	const Chunk::Coordinate coordinate{-3, 8, 11};

	Chunk::Protocol::Response::Builder builder(101u);
	builder.addSuccess(coordinate, chunk);
	const auto response = std::move(builder).build();

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
				cellOffset + index * sizeof(Voxel::Cell)),
			index + 1u);
	}
}

TEST(ChunkProtocolResponse, RejectedOnlyUsesEqualEmptySuccessBoundary)
{
	Chunk::Protocol::Response::Builder builder(102u);
	builder.addRejected({1, 2, 3});
	const auto response = std::move(builder).build();

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
	Chunk::Protocol::Response::Builder builder(103u);
	builder.addUnavailable({1, 2, 3});
	const auto response = std::move(builder).build();

	EXPECT_EQ(response.successOffset(), SummarySize);
	EXPECT_EQ(response.rejectedOffset(), SummarySize);
	EXPECT_EQ(response.unavailableOffset(), SummarySize);
	EXPECT_EQ(response.size(), SummarySize + ResultEntrySize);
	EXPECT_EQ(
		response.readAt<std::uint8_t>(
			response.unavailableOffset() + sizeof(Chunk::Coordinate)),
		static_cast<std::uint8_t>(Chunk::Protocol::Response::State::Unavailable));
}

TEST(ChunkProtocolResponse, BuilderEncodesGroupsAndCoordinatesCanonicallyRegardlessOfAddOrder)
{
	const Chunk chunk = makeEmptyChunk();

	Chunk::Protocol::Response::Builder builder(104u);
	builder.addUnavailable({9, 0, 0});
	builder.addRejected({3, 0, 0});
	builder.addSuccess({8, 0, 0}, chunk);
	builder.addRejected({-4, 5, 0});
	builder.addSuccess({-2, 7, 1}, chunk);
	builder.addUnavailable({0, 0, -1});
	const auto response = std::move(builder).build();

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

	Chunk::Protocol::Response::Builder firstBuilder(105u);
	firstBuilder.addUnavailable({7, 0, 0});
	firstBuilder.addSuccess({4, 0, 0}, chunk);
	firstBuilder.addRejected({-3, 0, 0});
	firstBuilder.addSuccess({-5, 0, 0}, chunk);
	const auto first = std::move(firstBuilder).build();

	Chunk::Protocol::Response::Builder secondBuilder(105u);
	secondBuilder.addSuccess({-5, 0, 0}, chunk);
	secondBuilder.addRejected({-3, 0, 0});
	secondBuilder.addSuccess({4, 0, 0}, chunk);
	secondBuilder.addUnavailable({7, 0, 0});
	const auto second = std::move(secondBuilder).build();

	expectPayloadsEqual(first, second);
}

TEST(ChunkProtocolResponse, RoundTripPreservesValidatedRanges)
{
	Chunk::Protocol::Response::Builder builder(106u);
	builder.addRejected({-5, 4, 3});
	builder.addUnavailable({8, 9, 10});
	const auto source = std::move(builder).build();

	const spk::Message raw = source;
	const Chunk::Protocol::Response decoded(raw);

	EXPECT_EQ(decoded.successOffset(), source.successOffset());
	EXPECT_EQ(decoded.rejectedOffset(), source.rejectedOffset());
	EXPECT_EQ(decoded.unavailableOffset(), source.unavailableOffset());
	EXPECT_EQ(
		decoded.readAt<Chunk::Coordinate>(decoded.rejectedOffset()),
		(Chunk::Coordinate{-5, 4, 3}));
	EXPECT_EQ(
		decoded.readAt<Chunk::Coordinate>(decoded.unavailableOffset()),
		(Chunk::Coordinate{8, 9, 10}));
}

TEST(ChunkProtocolResponse, ValidatedRangesSupportReadAtWithoutChangingCursor)
{
	Chunk::Protocol::Response::Builder builder(107u);
	builder.addRejected({-7, 2, 9});
	const auto source = std::move(builder).build();

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
		Chunk::Protocol::Response::Builder builder(108u);
		builder.addUnavailable({1, -2, 3});
		const auto source = std::move(builder).build();
		const spk::Message raw = source;
		return Chunk::Protocol::Response(raw);
	}();

	EXPECT_EQ(
		decoded.readAt<Chunk::Coordinate>(decoded.unavailableOffset()),
		(Chunk::Coordinate{1, -2, 3}));
}

TEST(ChunkProtocolResponse, BuilderRejectsDuplicateCoordinateAcrossStates)
{
	const Chunk chunk = makeEmptyChunk();
	Chunk::Protocol::Response::Builder builder(109u);
	builder.addSuccess({1, 2, 3}, chunk);

	EXPECT_THROW(builder.addRejected({1, 2, 3}), spk::Exception);
	EXPECT_THROW(builder.addUnavailable({1, 2, 3}), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsWrongMessageType)
{
	auto valid = emptyResponse(110u);
	spk::Message raw = valid;
	raw.setType(
		static_cast<spk::Message::Type>(Networking::MessageType::ChunkRequest));

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsZeroRequestID)
{
	auto valid = emptyResponse(111u);
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
	Chunk::Protocol::Response::Builder builder(118u);
	builder.addRejected({1, 2, 3});
	const auto valid = std::move(builder).build();

	spk::Message raw = valid;
	raw.append(std::uint8_t{0u});

	EXPECT_THROW((void)Chunk::Protocol::Response(raw), spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsKnownStateInWrongGroup)
{
	Chunk::Protocol::Response::Builder builder(119u);
	builder.addRejected({1, 2, 3});
	const auto valid = std::move(builder).build();

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
	Chunk::Protocol::Response::Builder builder(120u);
	builder.addRejected({1, 2, 3});
	const auto valid = std::move(builder).build();

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
