#include "erelia/core/networking/terrain_protocol.hpp"
#include <gtest/gtest.h>
#include <type_traits>

namespace
{
	struct AlternateColumn : public Column
	{
		struct Protocol
		{
			using MessageTypes = Networking::CollectionProtocol::MessageTypes<
				static_cast<Networking::MessageType>(101),
				static_cast<Networking::MessageType>(102),
				static_cast<Networking::MessageType>(103),
				static_cast<Networking::MessageType>(104)>;
		};
	};
	using AlternateProtocol = Networking::CollectionProtocol::Codec<Column::Coordinate, AlternateColumn>;
	static_assert(std::is_same_v<Networking::ChunkProtocol::Types, Chunk::Protocol::MessageTypes>);
	static_assert(std::is_same_v<Networking::ColumnProtocol::Types, Column::Protocol::MessageTypes>);
}

TEST(CollectionProtocolTypes, DomainDeclarationsPreserveWireIDs)
{
	using ChunkTypes = Chunk::Protocol::MessageTypes;
	using ColumnTypes = Column::Protocol::MessageTypes;
	EXPECT_EQ(ChunkTypes::Request, Networking::MessageType::ChunkRequest);
	EXPECT_EQ(ChunkTypes::Response, Networking::MessageType::ChunkResponse);
	EXPECT_EQ(ChunkTypes::Update, Networking::MessageType::ChunkUpdate);
	EXPECT_EQ(ChunkTypes::Error, Networking::MessageType::ChunkError);
	EXPECT_EQ(ColumnTypes::Request, Networking::MessageType::ColumnRequest);
	EXPECT_EQ(ColumnTypes::Response, Networking::MessageType::ColumnResponse);
	EXPECT_EQ(ColumnTypes::Update, Networking::MessageType::ColumnUpdate);
	EXPECT_EQ(ColumnTypes::Error, Networking::MessageType::ColumnError);
}

TEST(CollectionProtocolTypes, CustomDomainIDsDriveEveryMessageFamily)
{
	const auto request = AlternateProtocol::Request::build(7, {{-1, 2}});
	const auto response = AlternateProtocol::Response::build(7, {{{-1, 2}, AlternateColumn{}}});
	const auto update = AlternateProtocol::Update::build({}, {{-1, 2}});
	const auto error = AlternateProtocol::Error::build(7, {Networking::Diagnostic::Severity::Warning, "Custom"}, {{-1, 2}});
	EXPECT_EQ(request.type(), 101u);
	EXPECT_EQ(response.type(), 102u);
	EXPECT_EQ(update.type(), 103u);
	EXPECT_EQ(error.type(), 104u);
	EXPECT_EQ(request.keys(), (std::vector<Column::Coordinate>{{-1, 2}}));
	EXPECT_TRUE(response.section(0).success.front().element.chunks.empty());
	EXPECT_EQ(update.section(0, true).removed, request.keys());
	EXPECT_EQ(error.keys(), request.keys());
	EXPECT_THROW((void)Networking::ColumnProtocol::Request(request), spk::Exception);
	EXPECT_THROW((void)AlternateProtocol::Request(Networking::ColumnProtocol::Request::build(7, {{-1, 2}})), spk::Exception);
}
