#include "erelia/core/chunk_protocol.hpp"

#include <exception.hpp>
#include <gtest/gtest.h>

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <mutex>
#include <set>
#include <thread>
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

TEST(ChunkProtocolRequest, ConstructionOwnsTypeAndNonZeroRequestID)
{
	const Chunk::Protocol::Request first;
	const Chunk::Protocol::Request second;

	EXPECT_EQ(first.type(), requestMessageType());
	EXPECT_NE(first.requestID(), 0u);
	EXPECT_EQ(second.type(), requestMessageType());
	EXPECT_NE(second.requestID(), 0u);
	EXPECT_GT(second.requestID(), first.requestID());
	EXPECT_TRUE(first.empty());
	EXPECT_TRUE(second.empty());
}

TEST(ChunkProtocolRequest, GeneratedRequestIDsAreUniqueAcrossConcurrentConstruction)
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
				const Chunk::Protocol::Request request;

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

TEST(ChunkProtocolRequest, AddSerializesExactlyOneCoordinateWithoutCount)
{
	Chunk::Protocol::Request request;
	const Chunk::Coordinate coordinate{17, -4, 23};

	EXPECT_TRUE(request.add(coordinate));

	EXPECT_EQ(request.size(), sizeof(Chunk::Coordinate));
	EXPECT_EQ(request.readAt<Chunk::Coordinate>(0u), coordinate);
	EXPECT_EQ(request.coordinates(), (std::set<Chunk::Coordinate>{coordinate}));
	EXPECT_TRUE(request.duplicateCoordinates().empty());
}

TEST(ChunkProtocolRequest, PreservesWireInsertionOrderAndCoordinateValuesExactly)
{
	Chunk::Protocol::Request request;
	const std::vector<Chunk::Coordinate> expectedWireOrder = {
		{-123456, 789, -42},
		{0, -1, 1},
		{2147483647, -2147483647 - 1, 7}};

	for (const auto &coordinate : expectedWireOrder)
	{
		EXPECT_TRUE(request.add(coordinate));
	}

	ASSERT_EQ(request.size(), expectedWireOrder.size() * sizeof(Chunk::Coordinate));
	for (std::size_t index = 0; index < expectedWireOrder.size(); ++index)
	{
		EXPECT_EQ(
			request.readAt<Chunk::Coordinate>(index * sizeof(Chunk::Coordinate)),
			expectedWireOrder[index]);
	}

	EXPECT_EQ(
		request.coordinates(),
		(std::set<Chunk::Coordinate>(
			expectedWireOrder.begin(),
			expectedWireOrder.end())));
}

TEST(ChunkProtocolRequest, DuplicateAddIsRefusedWithoutChangingPayload)
{
	const Chunk::Coordinate a{1, 2, 3};
	const Chunk::Coordinate b{-4, 5, 6};
	const Chunk::Coordinate c{7, 8, -9};

	Chunk::Protocol::Request request;
	EXPECT_TRUE(request.add(a));
	EXPECT_TRUE(request.add(b));

	const auto sizeBeforeDuplicate = request.size();
	EXPECT_FALSE(request.add(a));
	EXPECT_EQ(request.size(), sizeBeforeDuplicate);

	EXPECT_TRUE(request.add(c));
	EXPECT_FALSE(request.add(b));
	EXPECT_FALSE(request.add(b));

	EXPECT_EQ(request.size(), 3u * sizeof(Chunk::Coordinate));
	EXPECT_EQ(
		request.coordinates(),
		(std::set<Chunk::Coordinate>{a, b, c}));
	EXPECT_TRUE(request.duplicateCoordinates().empty());
}

TEST(ChunkProtocolRequest, RawDuplicateInputKeepsUniqueResolutionAndDistinctDiagnostics)
{
	const Chunk::Coordinate a{1, 2, 3};
	const Chunk::Coordinate b{-4, 5, 6};
	const Chunk::Coordinate c{7, 8, -9};

	const spk::Message raw = requestMessage(
		79u,
		{a, b, a, c, b, b});
	const Chunk::Protocol::Request request(raw);

	EXPECT_EQ(
		request.coordinates(),
		(std::set<Chunk::Coordinate>{a, b, c}));
	EXPECT_EQ(
		request.duplicateCoordinates(),
		(std::set<Chunk::Coordinate>{a, b}));
	EXPECT_EQ(request.size(), 6u * sizeof(Chunk::Coordinate));
}

TEST(ChunkProtocolRequest, AcceptsExactly1024CoordinatesAndRejectsTheNextUniqueAdd)
{
	Chunk::Protocol::Request request;

	for (std::int32_t index = 0; index < 1024; ++index)
	{
		EXPECT_TRUE(request.add({index, -index, index * 2}));
	}

	EXPECT_EQ(request.size(), 1024u * sizeof(Chunk::Coordinate));
	EXPECT_EQ(request.coordinates().size(), 1024u);

	const auto previousSize = request.size();
	EXPECT_FALSE(request.add({0, 0, 0}));
	EXPECT_EQ(request.size(), previousSize);

	EXPECT_THROW((void)request.add({1024, -1024, 2048}), spk::Exception);
	EXPECT_EQ(request.size(), previousSize);
	EXPECT_EQ(request.coordinates().size(), 1024u);
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
	EXPECT_EQ(
		request.coordinates(),
		(std::set<Chunk::Coordinate>(
			coordinates.begin(),
			coordinates.end())));
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
	EXPECT_EQ(
		request.coordinates(),
		(std::set<Chunk::Coordinate>{{1, 2, 3}, {4, 5, 6}}));
}
