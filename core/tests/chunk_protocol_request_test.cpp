#include "erelia/core/chunk_protocol.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <mutex>
#include <set>
#include <thread>
#include <utility>
#include <vector>

namespace
{
	[[nodiscard]] spk::Message::Type requestMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(Networking::MessageType::ChunkRequest);
	}

	spk::Message requestMessage(
		spk::Message::RequestID requestID,
		const std::vector<Chunk::Coordinate> &coordinates)
	{
		spk::Message message(requestMessageType());
		message.setRequestID(requestID);

		for (const auto &coordinate : coordinates)
		{
			message.append(coordinate);
		}

		return message;
	}

	Chunk::Protocol::Request buildRequest(
		const std::vector<Chunk::Coordinate> &coordinates)
	{
		Chunk::Protocol::Request::Builder builder;
		for (const auto &coordinate : coordinates)
		{
			builder.add(coordinate);
		}
		return std::move(builder).build();
	}
}

TEST(ChunkProtocolMessageType, UsesApprovedTypedValues)
{
	EXPECT_EQ(
		static_cast<spk::Message::Type>(Networking::MessageType::ChunkRequest),
		1u);
	EXPECT_EQ(
		static_cast<spk::Message::Type>(Networking::MessageType::ChunkResponse),
		2u);
	EXPECT_EQ(
		static_cast<spk::Message::Type>(Networking::MessageType::ChunkError),
		3u);
}

TEST(ChunkProtocolRequest, BuilderProducesTypedRequestWithNonZeroMonotonicRequestID)
{
	const auto first = buildRequest({{1, 2, 3}});
	const auto second = buildRequest({{4, 5, 6}});

	EXPECT_EQ(first.type(), requestMessageType());
	EXPECT_NE(first.requestID(), 0u);
	EXPECT_EQ(second.type(), requestMessageType());
	EXPECT_NE(second.requestID(), 0u);
	EXPECT_GT(second.requestID(), first.requestID());
}

TEST(ChunkProtocolRequest, GeneratedRequestIDsAreUniqueAcrossConcurrentBuilds)
{
	constexpr std::size_t ThreadCount = 8u;
	constexpr std::size_t RequestsPerThread = 64u;

	std::mutex mutex;
	std::vector<spk::Message::RequestID> requestIDs;
	requestIDs.reserve(ThreadCount * RequestsPerThread);

	std::vector<std::thread> threads;
	threads.reserve(ThreadCount);

	for (std::size_t threadIndex = 0; threadIndex < ThreadCount; ++threadIndex)
	{
		threads.emplace_back([&] {
			for (std::size_t requestIndex = 0; requestIndex < RequestsPerThread; ++requestIndex)
			{
				Chunk::Protocol::Request::Builder builder;
				builder.add(
					{static_cast<std::int32_t>(requestIndex), 0, 0});
				const auto request = std::move(builder).build();

				std::scoped_lock lock(mutex);
				requestIDs.push_back(request.requestID());
			}
		});
	}

	for (auto &thread : threads)
	{
		thread.join();
	}

	ASSERT_EQ(requestIDs.size(), ThreadCount * RequestsPerThread);
	EXPECT_TRUE(
		std::ranges::none_of(
			requestIDs,
			[](spk::Message::RequestID requestID) {
				return requestID == 0u;
			}));

	std::ranges::sort(requestIDs);
	EXPECT_EQ(
		std::adjacent_find(requestIDs.begin(), requestIDs.end()),
		requestIDs.end());
}

TEST(ChunkProtocolRequest, BuilderRejectsEmptyRequest)
{
	Chunk::Protocol::Request::Builder builder;

	EXPECT_THROW(
		(void)std::move(builder).build(),
		spk::Exception);
}

TEST(ChunkProtocolRequest, BuildSerializesExactlyOneCoordinateWithoutCount)
{
	const Chunk::Coordinate coordinate{17, -4, 23};

	auto request = buildRequest({coordinate});

	EXPECT_EQ(request.size(), sizeof(Chunk::Coordinate));
	EXPECT_EQ(request.coordinateCount(), 1u);
	EXPECT_EQ(request.coordinate(0u), coordinate);
	EXPECT_EQ(request.readAt<Chunk::Coordinate>(0u), coordinate);
	EXPECT_TRUE(request.duplicateCoordinates().empty());
}

TEST(ChunkProtocolRequest, BuilderPreservesInsertionOrderAndCoordinateValuesExactly)
{
	const std::vector<Chunk::Coordinate> expected = {
		{-123456, 789, -42},
		{0, -1, 1},
		{2147483647, -2147483647 - 1, 7}};

	auto request = buildRequest(expected);

	ASSERT_EQ(request.coordinateCount(), expected.size());
	ASSERT_EQ(request.size(), expected.size() * sizeof(Chunk::Coordinate));

	for (std::size_t index = 0; index < expected.size(); ++index)
	{
		EXPECT_EQ(request.coordinate(index), expected[index]);
	}
}

TEST(ChunkProtocolRequest, BuilderDuplicateCheckIsDebugOnly)
{
	const Chunk::Coordinate coordinate{1, 2, 3};

	Chunk::Protocol::Request::Builder builder;
	builder.add(coordinate);

#ifndef NDEBUG
	EXPECT_THROW(builder.add(coordinate), spk::Exception);

	const auto request = std::move(builder).build();
	EXPECT_EQ(request.coordinateCount(), 1u);
	EXPECT_TRUE(request.duplicateCoordinates().empty());
#else
	EXPECT_NO_THROW(builder.add(coordinate));

	const auto request = std::move(builder).build();
	EXPECT_EQ(request.coordinateCount(), 2u);
	EXPECT_EQ(
		request.duplicateCoordinates(),
		(std::set<Chunk::Coordinate>{coordinate}));
#endif
}

TEST(ChunkProtocolRequest, IncomingDuplicatePayloadExposesDistinctDuplicateCoordinates)
{
	const Chunk::Coordinate a{1, 2, 3};
	const Chunk::Coordinate b{-4, 5, 6};
	const Chunk::Coordinate c{7, 8, -9};

	const spk::Message raw = requestMessage(
		79u,
		{a, b, a, c, b, b});
	const Chunk::Protocol::Request request(raw);

	EXPECT_EQ(request.coordinateCount(), 6u);
	EXPECT_EQ(request.coordinate(0u), a);
	EXPECT_EQ(request.coordinate(1u), b);
	EXPECT_EQ(request.coordinate(2u), a);
	EXPECT_EQ(
		request.duplicateCoordinates(),
		(std::set<Chunk::Coordinate>{a, b}));
}

TEST(ChunkProtocolRequest, AcceptsExactly1024CoordinatesAndRejectsTheNextAdd)
{
	Chunk::Protocol::Request::Builder builder;

	for (std::int32_t index = 0; index < 1024; ++index)
	{
		EXPECT_NO_THROW(builder.add({index, -index, index * 2}));
	}

	EXPECT_THROW(
		builder.add({1024, -1024, 2048}),
		spk::Exception);

	const auto request = std::move(builder).build();
	EXPECT_EQ(request.coordinateCount(), 1024u);
	EXPECT_EQ(request.size(), 1024u * sizeof(Chunk::Coordinate));
}

TEST(ChunkProtocolRequest, DecodesThe1024CoordinateBoundary)
{
	std::vector<Chunk::Coordinate> coordinates;
	coordinates.reserve(1024u);

	for (std::int32_t index = 0; index < 1024; ++index)
	{
		coordinates.push_back({index, 0, -index});
	}

	const spk::Message raw = requestMessage(77u, coordinates);
	const Chunk::Protocol::Request request(raw);

	EXPECT_EQ(request.requestID(), 77u);
	EXPECT_EQ(request.coordinateCount(), coordinates.size());
	for (std::size_t index = 0; index < coordinates.size(); ++index)
	{
		EXPECT_EQ(request.coordinate(index), coordinates[index]);
	}
	EXPECT_TRUE(request.duplicateCoordinates().empty());
}

TEST(ChunkProtocolRequest, RejectsDerived1025CoordinatePayload)
{
	std::vector<Chunk::Coordinate> coordinates;
	coordinates.reserve(1025u);

	for (std::int32_t index = 0; index < 1025; ++index)
	{
		coordinates.push_back({index, index, index});
	}

	const spk::Message raw = requestMessage(78u, coordinates);

	EXPECT_THROW((void)Chunk::Protocol::Request(raw), spk::Exception);
}

TEST(ChunkProtocolRequest, RejectsWrongMessageType)
{
	spk::Message raw(
		static_cast<spk::Message::Type>(Networking::MessageType::ChunkResponse));
	raw.setRequestID(1u);
	raw.append(Chunk::Coordinate{1, 2, 3});

	EXPECT_THROW((void)Chunk::Protocol::Request(raw), spk::Exception);
}

TEST(ChunkProtocolRequest, RejectsZeroRequestID)
{
	spk::Message raw(requestMessageType());
	raw.append(Chunk::Coordinate{1, 2, 3});

	EXPECT_THROW((void)Chunk::Protocol::Request(raw), spk::Exception);
}

TEST(ChunkProtocolRequest, RejectsEmptyIncomingPayload)
{
	spk::Message raw(requestMessageType());
	raw.setRequestID(1u);

	EXPECT_THROW((void)Chunk::Protocol::Request(raw), spk::Exception);
}

TEST(ChunkProtocolRequest, RejectsMisalignedAndTruncatedCoordinatePayload)
{
	spk::Message raw(requestMessageType());
	raw.setRequestID(1u);

	const Chunk::Coordinate coordinate{1, 2, 3};
	raw.append(&coordinate, sizeof(coordinate) - 1u);

	EXPECT_THROW((void)Chunk::Protocol::Request(raw), spk::Exception);
}

TEST(ChunkProtocolRequest, CoordinateRejectsOutOfRangeIndex)
{
	const auto request = buildRequest({{1, 2, 3}});

	EXPECT_THROW((void)request.coordinate(1u), spk::Exception);
}

TEST(ChunkProtocolRequest, AccessorsReadTheMessagePayloadAsTheirSourceOfTruth)
{
	auto request = buildRequest(
		{{1, 2, 3}, {4, 5, 6}});
	const Chunk::Coordinate replacement{-7, 8, 9};

	request.edit(0u, replacement);

	EXPECT_EQ(request.coordinate(0u), replacement);
	EXPECT_EQ(request.coordinate(1u), (Chunk::Coordinate{4, 5, 6}));
	EXPECT_TRUE(request.duplicateCoordinates().empty());
}

TEST(ChunkProtocolRequest, DecodeUsesCursorIndependentReads)
{
	spk::Message raw = requestMessage(
		45u,
		{{1, 2, 3}, {4, 5, 6}});
	raw.skip<Chunk::Coordinate>();
	const auto originalReadOffset = raw.readOffset();

	const Chunk::Protocol::Request request(raw);

	EXPECT_EQ(raw.readOffset(), originalReadOffset);
	EXPECT_EQ(request.readOffset(), originalReadOffset);
	EXPECT_EQ(request.coordinate(0u), (Chunk::Coordinate{1, 2, 3}));
	EXPECT_EQ(request.coordinate(1u), (Chunk::Coordinate{4, 5, 6}));
}
