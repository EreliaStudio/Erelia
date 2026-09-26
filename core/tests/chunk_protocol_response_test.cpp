#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/chunk_protocol_response.hpp"
#include "erelia/core/networking/message_type.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <string>
#include <utility>

namespace
{
	constexpr std::size_t SummarySize =
		sizeof(std::uint32_t);
	constexpr std::size_t CellCount =
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent);
	constexpr std::size_t CellBytes =
		CellCount * sizeof(Voxel::Cell::PackedType);
	constexpr std::size_t SuccessEntrySize =
		sizeof(Chunk::Coordinate) + CellBytes;
	constexpr std::size_t FailureFixedSize =
		sizeof(Chunk::Coordinate) +
		sizeof(std::uint8_t) +
		sizeof(std::uint32_t);

	[[nodiscard]] spk::Message::Type responseMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(
			Networking::MessageType::ChunkResponse);
	}

	Chunk makeEmptyChunk()
	{
		Chunk::Builder builder;
		return std::move(builder).build();
	}

	Chunk makeChunk(Voxel::Cell::PackedType packed)
	{
		Chunk::Builder builder;
		(void)builder.set(
			{0, 0, 0},
			Voxel::Cell(packed));
		return std::move(builder).build();
	}

	Chunk makeSequentialChunk()
	{
		Chunk::Builder builder;
		for (std::int32_t z = 0; z < Chunk::Extent; ++z)
		{
			for (std::int32_t x = 0; x < Chunk::Extent; ++x)
			{
				for (std::int32_t y = 0; y < Chunk::Extent; ++y)
				{
					const auto index =
						static_cast<std::uint32_t>(
							y +
							Chunk::Extent *
								(x +
								 Chunk::Extent * z));
					(void)builder.set(
						{x, y, z},
						Voxel::Cell(index + 1u));
				}
			}
		}
		return std::move(builder).build();
	}

	spk::Message responseMessage(
		spk::Message::RequestID requestID)
	{
		spk::Message result(responseMessageType());
		result.setRequestID(requestID);
		return result;
	}

	void appendFailure(
		spk::Message &message,
		const Chunk::Coordinate &coordinate,
		std::uint8_t code,
		const std::string &text)
	{
		message << coordinate;
		message << code;
		message << text;
	}

	void expectPayloadsEqual(
		const spk::Message &first,
		const spk::Message &second)
	{
		ASSERT_EQ(first.size(), second.size());
		EXPECT_TRUE(
			std::ranges::equal(
				first.data(),
				second.data()));
	}
}

TEST(ChunkProtocolResponse, EmptyBuilderProducesHeaderOnlyResponse)
{
	Chunk::Protocol::Response::Builder builder(100u);
	const auto response = std::move(builder).build();

	EXPECT_EQ(response.type(), responseMessageType());
	EXPECT_EQ(response.requestID(), 100u);
	EXPECT_EQ(response.size(), SummarySize);
	EXPECT_EQ(response.failureOffset(), SummarySize);
	EXPECT_EQ(response.successCount(), 0u);
	EXPECT_EQ(response.failureCount(), 0u);
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

	EXPECT_EQ(
		response.failureOffset(),
		SummarySize + SuccessEntrySize);
	EXPECT_EQ(
		response.size(),
		SummarySize + SuccessEntrySize);
	ASSERT_EQ(response.successCount(), 1u);
	EXPECT_EQ(response.failureCount(), 0u);

	const auto success = response.success(0u);
	EXPECT_EQ(success.coordinate, coordinate);
	EXPECT_EQ(
		success.chunk.cells().front().packed(),
		1u);
	EXPECT_EQ(
		success.chunk.cells().back().packed(),
		CellCount);

	const std::size_t cellOffset =
		SummarySize + sizeof(Chunk::Coordinate);
	for (std::size_t index = 0u; index < CellCount; ++index)
	{
		EXPECT_EQ(
			response.readAt<Voxel::Cell::PackedType>(
				cellOffset +
				index *
					sizeof(Voxel::Cell::PackedType)),
			index + 1u);
	}
}

TEST(ChunkProtocolResponse, FailureOnlyUsesSparkleStringEncoding)
{
	const Chunk::Coordinate coordinate{1, -2, 3};
	const std::string text = "generation failed";

	Chunk::Protocol::Response::Builder builder(102u);
	builder.addFailure(
		coordinate,
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed,
		text);
	const auto response = std::move(builder).build();

	EXPECT_EQ(response.failureOffset(), SummarySize);
	EXPECT_EQ(response.successCount(), 0u);
	ASSERT_EQ(response.failureCount(), 1u);
	EXPECT_EQ(
		response.size(),
		SummarySize +
		FailureFixedSize +
		text.size());

	const auto failure = response.failure(0u);
	EXPECT_EQ(failure.coordinate, coordinate);
	EXPECT_EQ(
		failure.code,
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed);
	EXPECT_EQ(failure.message, text);
	EXPECT_EQ(
		response.readAt<std::uint32_t>(
			SummarySize +
			sizeof(Chunk::Coordinate) +
			sizeof(std::uint8_t)),
		text.size());
}

TEST(ChunkProtocolResponse, BuilderSortsBothSectionsRegardlessOfAddOrder)
{
	const Chunk chunk = makeEmptyChunk();

	Chunk::Protocol::Response::Builder builder(103u);
	builder.addFailure(
		{9, 0, 0},
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed,
		"nine");
	builder.addSuccess({8, 0, 0}, chunk);
	builder.addFailure(
		{-4, 5, 0},
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed,
		"minus");
	builder.addSuccess({-2, 7, 1}, chunk);
	const auto response = std::move(builder).build();

	ASSERT_EQ(response.successCount(), 2u);
	ASSERT_EQ(response.failureCount(), 2u);
	EXPECT_EQ(
		response.success(0u).coordinate,
		(Chunk::Coordinate{-2, 7, 1}));
	EXPECT_EQ(
		response.success(1u).coordinate,
		(Chunk::Coordinate{8, 0, 0}));
	EXPECT_EQ(
		response.failure(0u).coordinate,
		(Chunk::Coordinate{-4, 5, 0}));
	EXPECT_EQ(
		response.failure(1u).coordinate,
		(Chunk::Coordinate{9, 0, 0}));
}

TEST(ChunkProtocolResponse, SameLogicalResultsProduceIdenticalPayloadBytes)
{
	const Chunk chunk = makeChunk(77u);

	Chunk::Protocol::Response::Builder firstBuilder(104u);
	firstBuilder.addFailure(
		{7, 0, 0},
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed,
		"failure");
	firstBuilder.addSuccess({4, 0, 0}, chunk);
	firstBuilder.addSuccess({-5, 0, 0}, chunk);
	const auto first = std::move(firstBuilder).build();

	Chunk::Protocol::Response::Builder secondBuilder(104u);
	secondBuilder.addSuccess({-5, 0, 0}, chunk);
	secondBuilder.addSuccess({4, 0, 0}, chunk);
	secondBuilder.addFailure(
		{7, 0, 0},
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed,
		"failure");
	const auto second = std::move(secondBuilder).build();

	expectPayloadsEqual(first, second);
}

TEST(ChunkProtocolResponse, RoundTripPreservesSemanticAccessors)
{
	Chunk::Protocol::Response::Builder builder(105u);
	builder.addSuccess(
		{-5, 4, 3},
		makeChunk(88u));
	builder.addFailure(
		{8, 9, 10},
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed,
		"failure");
	const auto source = std::move(builder).build();

	const spk::Message raw = source;
	const Chunk::Protocol::Response decoded(raw);

	EXPECT_EQ(
		decoded.failureOffset(),
		source.failureOffset());
	ASSERT_EQ(decoded.successCount(), 1u);
	ASSERT_EQ(decoded.failureCount(), 1u);
	EXPECT_EQ(
		decoded.success(0u).coordinate,
		(Chunk::Coordinate{-5, 4, 3}));
	EXPECT_EQ(
		decoded.success(0u).chunk.at({0, 0, 0}).packed(),
		88u);
	EXPECT_EQ(
		decoded.failure(0u).coordinate,
		(Chunk::Coordinate{8, 9, 10}));
	EXPECT_EQ(
		decoded.failure(0u).message,
		"failure");
}

TEST(ChunkProtocolResponse, DecodingAndAccessorsDoNotMoveSourceCursor)
{
	Chunk::Protocol::Response::Builder builder(106u);
	builder.addFailure(
		{-7, 2, 9},
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed,
		"failure");
	const auto source = std::move(builder).build();

	spk::Message raw = source;
	raw.skip<std::uint32_t>();
	const auto originalReadOffset =
		raw.readOffset();

	const Chunk::Protocol::Response decoded(raw);
	(void)decoded.failure(0u);

	EXPECT_EQ(
		decoded.readOffset(),
		originalReadOffset);
	EXPECT_EQ(
		raw.readOffset(),
		originalReadOffset);
}

TEST(ChunkProtocolResponse, BuilderRejectsDuplicateCoordinateAcrossSections)
{
	const Chunk chunk = makeEmptyChunk();
	Chunk::Protocol::Response::Builder builder(107u);
	builder.addSuccess({1, 2, 3}, chunk);

	EXPECT_THROW(
		builder.addFailure(
			{1, 2, 3},
			Chunk::Protocol::Response::Failure::Code::
				AcquisitionFailed,
			"failure"),
		spk::Exception);
}

TEST(ChunkProtocolResponse, BuilderRejectsUnknownFailureCode)
{
	Chunk::Protocol::Response::Builder builder(108u);

	EXPECT_THROW(
		builder.addFailure(
			{1, 2, 3},
			static_cast<
				Chunk::Protocol::Response::Failure::Code>(
				99u),
			"failure"),
		spk::Exception);
}

TEST(ChunkProtocolResponse, AccessorsRejectOutOfRangeIndices)
{
	Chunk::Protocol::Response::Builder builder(109u);
	const auto response = std::move(builder).build();

	EXPECT_THROW(
		(void)response.success(0u),
		spk::Exception);
	EXPECT_THROW(
		(void)response.failure(0u),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsWrongMessageType)
{
	Chunk::Protocol::Response::Builder builder(110u);
	auto valid = std::move(builder).build();
	spk::Message raw = valid;
	raw.setType(
		static_cast<spk::Message::Type>(
			Networking::MessageType::ChunkRequest));

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsZeroRequestID)
{
	Chunk::Protocol::Response::Builder builder(111u);
	auto valid = std::move(builder).build();
	spk::Message raw = valid;
	raw.setRequestID(0u);

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsMissingFailureOffset)
{
	spk::Message raw = responseMessage(112u);

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsFailureOffsetBeforeHeader)
{
	spk::Message raw = responseMessage(113u);
	raw << std::uint32_t{0u};

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsFailureOffsetBeyondPayload)
{
	spk::Message raw = responseMessage(114u);
	raw << std::uint32_t{100u};

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsMisalignedSuccessSection)
{
	spk::Message raw = responseMessage(115u);
	raw << std::uint32_t{
		static_cast<std::uint32_t>(
			SummarySize + 1u)};
	raw << std::uint8_t{0u};

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsTruncatedFailureFixedFields)
{
	spk::Message raw = responseMessage(116u);
	raw << std::uint32_t{
		static_cast<std::uint32_t>(SummarySize)};
	raw << Chunk::Coordinate{1, 2, 3};
	raw << static_cast<std::uint8_t>(
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed);

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsUnknownFailureCode)
{
	spk::Message raw = responseMessage(117u);
	raw << std::uint32_t{
		static_cast<std::uint32_t>(SummarySize)};
	appendFailure(
		raw,
		{1, 2, 3},
		99u,
		"failure");

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsTruncatedFailureMessage)
{
	spk::Message raw = responseMessage(118u);
	raw << std::uint32_t{
		static_cast<std::uint32_t>(SummarySize)};
	raw << Chunk::Coordinate{1, 2, 3};
	raw << static_cast<std::uint8_t>(
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed);
	raw << std::uint32_t{5u};
	raw.append("abc", 3u);

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsDuplicateCoordinateAcrossSections)
{
	Chunk::Protocol::Response::Builder builder(119u);
	builder.addSuccess(
		{1, 2, 3},
		makeEmptyChunk());
	const auto valid = std::move(builder).build();

	spk::Message raw = valid;
	appendFailure(
		raw,
		{1, 2, 3},
		static_cast<std::uint8_t>(
			Chunk::Protocol::Response::Failure::Code::
				AcquisitionFailed),
		"failure");

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsUnsortedSuccessCoordinates)
{
	const Chunk chunk = makeEmptyChunk();
	Chunk::Protocol::Response::Builder builder(120u);
	builder.addSuccess({1, 0, 0}, chunk);
	builder.addSuccess({2, 0, 0}, chunk);
	auto valid = std::move(builder).build();

	spk::Message raw = valid;
	raw.edit(
		SummarySize,
		Chunk::Coordinate{2, 0, 0});
	raw.edit(
		SummarySize + SuccessEntrySize,
		Chunk::Coordinate{1, 0, 0});

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}

TEST(ChunkProtocolResponse, RejectsUnsortedFailureCoordinates)
{
	Chunk::Protocol::Response::Builder builder(121u);
	builder.addFailure(
		{1, 0, 0},
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed,
		"a");
	builder.addFailure(
		{2, 0, 0},
		Chunk::Protocol::Response::Failure::Code::
			AcquisitionFailed,
		"b");
	auto valid = std::move(builder).build();

	spk::Message raw = valid;
	const std::size_t firstOffset =
		valid.failureOffset();
	const std::size_t secondOffset =
		firstOffset +
		FailureFixedSize +
		1u;
	raw.edit(
		firstOffset,
		Chunk::Coordinate{2, 0, 0});
	raw.edit(
		secondOffset,
		Chunk::Coordinate{1, 0, 0});

	EXPECT_THROW(
		(void)Chunk::Protocol::Response(raw),
		spk::Exception);
}
