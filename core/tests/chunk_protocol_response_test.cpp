#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/networking/terrain_protocol.hpp"
#include <gtest/gtest.h>
namespace
{
	Chunk makeChunk()
	{
		Chunk::Builder builder;
		(void)builder.set({15, 14, 13}, Voxel::Cell(0xf1234567u));
		return std::move(builder).build();
	}
	spk::Message edited(const spk::Message &message, std::size_t offset, std::uint32_t value)
	{
		spk::Message copy = message;
		spk::Message::Writer writer(std::move(copy));
		writer.edit(offset, value);
		return std::move(writer).build();
	}
}
TEST(CollectionProtocolResponse, FixedChunkCodecAndIndependentReaders)
{
	const auto chunk = makeChunk();
	spk::Message::Writer writer;
	writer << chunk;
	auto message = std::move(writer).build();
	EXPECT_EQ(message.size(), chunk.cells().size_bytes());
	Chunk first, second;
	message.reader() >> first;
	message.reader() >> second;
	EXPECT_EQ(first.at({15, 14, 13}).packed(), 0xf1234567u);
	EXPECT_EQ(second.at({15, 14, 13}).packed(), 0xf1234567u);
	spk::Message::Writer shortWriter;
	shortWriter.resize(message.size() - 1);
	auto truncated = std::move(shortWriter).build();
	EXPECT_THROW(truncated.reader() >> first, spk::Exception);
	EXPECT_EQ(first.at({15, 14, 13}).packed(), 0xf1234567u);
}
TEST(CollectionProtocolResponse, OffsetTableCoversBothDomainsAndBoundary)
{
	using Protocol = Networking::ColumnProtocol;
	std::vector<Protocol::Success> success;
	std::vector<Protocol::Failed> failure;
	for (std::size_t index = 0; index < Column::ElementsPerResponseSection + 1; ++index)
	{
		success.push_back({{static_cast<int>(index), 0}, Column{{{static_cast<int>(index), 0, 0}}}});
		failure.push_back({{static_cast<int>(index), 1}, {Protocol::Failure::Code::AcquisitionFailed, "Rejected"}});
	}
	const auto response = Protocol::Response::build(5, success, failure);
	EXPECT_EQ(response.sectionCount(), 4u);
	EXPECT_EQ(response.firstSectionCount(), 2u);
	EXPECT_EQ(response.failureOffset(), response.offset(2));
	EXPECT_EQ(response.offset(4), response.size());
	EXPECT_EQ(response.section(0).success.size(), Column::ElementsPerResponseSection);
	EXPECT_EQ(response.section(1).success.size(), 1u);
	EXPECT_EQ(response.section(2).failure.size(), Column::ElementsPerResponseSection);
	EXPECT_EQ(response.section(3).failure.front().failure.message, "Rejected");
	using ChunkProtocol = Networking::ChunkProtocol;
	auto chunks = ChunkProtocol::Response::build(9, {{{-1, 0, 2}, makeChunk()}}, {{{1, 0, 2}, {ChunkProtocol::Failure::Code::AcquisitionFailed, "Failure"}}});
	EXPECT_EQ(chunks.sectionCount(), 2u);
	EXPECT_EQ(chunks.section(0).success.front().element.at({15, 14, 13}).packed(), 0xf1234567u);
	EXPECT_EQ(chunks.section(1).failure.front().failure.message, "Failure");
}
TEST(CollectionProtocolResponse, EmptySuccessFailureAndDeterministicOrder)
{
	using P = Networking::ColumnProtocol;
	auto empty = P::Response::build(1, {});
	EXPECT_EQ(empty.sectionCount(), 0u);
	EXPECT_EQ(empty.failureOffset(), empty.size());
	auto a = P::Response::build(2, {{{3, 0}, {}}, {{1, 0}, {}}});
	auto b = P::Response::build(2, {{{1, 0}, {}}, {{3, 0}, {}}});
	EXPECT_TRUE(std::equal(a.data().begin(), a.data().end(), b.data().begin(), b.data().end()));
	auto failed = P::Response::build(1, {}, {{{1, 1}, {P::Failure::Code::AcquisitionFailed, "Error"}}});
	EXPECT_EQ(failed.firstSectionCount(), 0u);
	EXPECT_EQ(failed.failureOffset(), failed.offset(0));
	EXPECT_THROW((void)P::Response::build(1, {{{1, 0}, {}}, {{1, 0}, {}}}), spk::Exception);
	EXPECT_THROW((void)P::Response::build(0, {}), spk::Exception);
}
TEST(CollectionProtocolResponse, RejectsMalformedOffsetsAndTruncatedSections)
{
	using P = Networking::ColumnProtocol;
	auto response = P::Response::build(1, {{{1, 0}, Column{{{1, 0, 0}}}}});
	for (const auto &[offset, value] : std::vector<std::pair<std::size_t, std::uint32_t>>{{0, 1000}, {4, 2}, {8, 0}, {12, 0}, {12, static_cast<std::uint32_t>(response.size() + 1)}})
	{
		EXPECT_THROW((void)P::Response(edited(response, offset, value)), spk::Exception);
	}
	spk::Message copy = response;
	spk::Message::Writer writer(std::move(copy));
	writer.resize(response.size() - 1);
	writer.edit(12, static_cast<std::uint32_t>(writer.size()));
	EXPECT_THROW(P::Response(std::move(writer).build()).validateEntries(), spk::Exception);
	EXPECT_THROW((void)response.section(1), spk::Exception);
}
TEST(CollectionProtocolResponse, RejectsSectionExceedingDomainLimit)
{
	using P = Networking::ColumnProtocol;
	std::vector<P::Success> values;
	for (std::size_t index = 0; index <= Column::ElementsPerResponseSection; ++index)
	{
		values.push_back({{static_cast<int>(index), 0}, {}});
	}
	auto response = P::Response::build(1, values);
	// Merge two sections into one, retaining valid outer bounds but violating the element limit.
	spk::Message::Writer writer(static_cast<spk::Message::Type>(Networking::MessageType::ColumnResponse));
	writer.setRequestID(1);
	writer << std::uint32_t{1} << std::uint32_t{1} << std::uint32_t{16} << static_cast<std::uint32_t>(16 + response.size() - response.offset(0));
	writer.append(response.data().data() + response.offset(0), response.size() - response.offset(0));
	EXPECT_THROW(P::Response(std::move(writer).build()).validateEntries(), spk::Exception);
}
