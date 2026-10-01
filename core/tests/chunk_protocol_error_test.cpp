#include "erelia/core/networking/terrain_protocol.hpp"
#include <gtest/gtest.h>
TEST(CollectionProtocolError, DiagnosticPrefixAndContextRoundTrip)
{
	using P = Networking::ChunkProtocol;
	auto error = P::Error::build(4, {Networking::Diagnostic::Severity::Warning, "Chunk_Coordinates_Duplication"}, {{-1, 0, 1}, {2, 0, 3}});
	EXPECT_EQ(error.requestID(), 4u);
	EXPECT_EQ(error.diagnostic().severity, Networking::Diagnostic::Severity::Warning);
	EXPECT_EQ(error.keys(), (std::vector<Chunk::Coordinate>{{-1, 0, 1}, {2, 0, 3}}));
	auto uncorrelated = Networking::ColumnProtocol::Error::build(0, {Networking::Diagnostic::Severity::Error, "Malformed"});
	EXPECT_EQ(uncorrelated.keys().size(), 0u);
	EXPECT_EQ(uncorrelated.requestID(), 0u);
}
TEST(CollectionProtocolError, RejectsTruncationTrailingDataAndUnknownSeverity)
{
	using P = Networking::ColumnProtocol;
	auto error = P::Error::build(1, {Networking::Diagnostic::Severity::Info, "Key"}, {{1, 2}});
	for (std::size_t size : {std::size_t{0}, error.size() - 1, error.size() + 1})
	{
		spk::Message copy = error;
		spk::Message::Writer writer(std::move(copy));
		writer.resize(size);
		EXPECT_THROW((void)P::Error(std::move(writer).build()), spk::Exception);
	}
	spk::Message copy = error;
	spk::Message::Writer writer(std::move(copy));
	writer.edit(0, std::uint8_t{5});
	EXPECT_THROW((void)P::Error(std::move(writer).build()), spk::Exception);
}
TEST(CollectionProtocolUpdate, UsesZeroCorrelationAndCommonSections)
{
	auto update = Networking::ColumnProtocol::Update::build({{{-1, 0}, Column{{{-1, 0, 0}}}}}, {{1, 2}});
	EXPECT_EQ(update.requestID(), 0u);
	EXPECT_EQ(update.sectionCount(), 2u);
	EXPECT_EQ(update.section(0, true).success.front().element.chunks.front(), (Chunk::Coordinate{-1, 0, 0}));
	EXPECT_EQ(update.section(1, true).removed.front(), (Column::Coordinate{1, 2}));
	spk::Message copy = update;
	spk::Message::Writer writer(std::move(copy));
	writer.setRequestID(3);
	EXPECT_THROW((void)Networking::ColumnProtocol::Update(std::move(writer).build()), spk::Exception);
}
