#include "erelia/core/networking/terrain_protocol.hpp"
#include <gtest/gtest.h>
namespace
{
	template <typename T>
	concept HasDiagnosticID = requires { T::Diagnostic; };
	static_assert(HasDiagnosticID<Networking::MessageType> == false);
	static_assert(sizeof(spk::Message::RequestID) == sizeof(std::uint64_t));
}
TEST(CollectionProtocolRequest, FamilyIDsAndChunkColumnRoundTrips)
{
	EXPECT_EQ(static_cast<unsigned>(Networking::MessageType::ChunkRequest), 1u);
	EXPECT_EQ(static_cast<unsigned>(Networking::MessageType::ChunkUpdate), 3u);
	EXPECT_EQ(static_cast<unsigned>(Networking::MessageType::ChunkError), 4u);
	EXPECT_EQ(static_cast<unsigned>(Networking::MessageType::ColumnRequest), 5u);
	EXPECT_EQ(static_cast<unsigned>(Networking::MessageType::ColumnError), 8u);
	auto chunks = Networking::ChunkProtocol::Request::build(1, {{-1, 2, -3}, {4, 5, 6}});
	auto columns = Networking::ColumnProtocol::Request::build(1, {{-1, -3}, {4, 6}});
	EXPECT_EQ(chunks.requestID(), 1u);
	EXPECT_EQ(columns.requestID(), 1u);
	EXPECT_EQ(chunks.keys(), (std::vector<Chunk::Coordinate>{{-1, 2, -3}, {4, 5, 6}}));
	EXPECT_EQ(columns.keys(), (std::vector<Column::Coordinate>{{-1, -3}, {4, 6}}));
}
TEST(CollectionProtocolRequest, DomainBoundariesAndMalformedMessages)
{
	using Request = Networking::ChunkProtocol::Request;
	std::vector<Chunk::Coordinate> keys(Chunk::MaximumElementsPerRequest);
	EXPECT_EQ(Request::build(1, keys).keys().size(), Chunk::MaximumElementsPerRequest);
	keys.push_back({});
	EXPECT_THROW((void)Request::build(1, keys), spk::Exception);
	EXPECT_THROW((void)Request::build(1, {}), spk::Exception);
	EXPECT_THROW((void)Request::build(0, {{1, 2, 3}}), spk::Exception);
	spk::Message::Writer writer(static_cast<spk::Message::Type>(Networking::MessageType::ChunkRequest));
	writer.setRequestID(1);
	writer << std::uint8_t{2};
	EXPECT_THROW((void)Request(std::move(writer).build()), spk::Exception);
	EXPECT_THROW((void)Request(Networking::ColumnProtocol::Request::build(1, {{1, 2}})), spk::Exception);
}
TEST(Column, RoundTripSparseNegativeCoordinatesAndRejectsInvalidMembership)
{
	Column input{{{-3, -2, -1}, {-3, 0, -1}, {-3, 4, -1}}};
	spk::Message::Writer writer;
	writer << input;
	auto message = std::move(writer).build();
	Column output;
	message.reader() >> output;
	EXPECT_EQ(output.chunks, input.chunks);
	spk::Message::Writer invalid;
	EXPECT_THROW(invalid << (Column{{{1, 0, 1}, {2, 1, 1}}}), spk::Exception);
	EXPECT_THROW(invalid << (Column{{{1, 1, 1}, {1, 0, 1}}}), spk::Exception);
	spk::Message::Writer truncated;
	truncated << std::uint32_t{100};
	auto shortMessage = std::move(truncated).build();
	EXPECT_THROW(shortMessage.reader() >> output, spk::Exception);
	EXPECT_EQ(output.chunks, input.chunks);
}
