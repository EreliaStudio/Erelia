#include "erelia/core/chunk_builder.hpp"
#include <exception.hpp>
Chunk::Chunk() :
	Chunk(std::move(Chunk::Builder{}).build())
{
}
spk::Message::Writer &operator<<(spk::Message::Writer &writer, const Chunk &chunk)
{
	if (chunk.cells().size() != static_cast<std::size_t>(Chunk::Extent * Chunk::Extent * Chunk::Extent))
	{
		throw spk::Exception("Invalid Chunk Cell count");
	}
	writer.append(chunk.cells().data(), chunk.cells().size_bytes());
	return writer;
}
const spk::Message::Reader &operator>>(const spk::Message::Reader &reader, Chunk &chunk)
{
	constexpr std::size_t count = Chunk::Extent * Chunk::Extent * Chunk::Extent;
	if (reader.size() - reader.readOffset() < count * sizeof(Voxel::Cell))
	{
		throw spk::Exception("Truncated Chunk");
	}
	Chunk::Builder builder;
	for (std::int32_t z = 0; z < Chunk::Extent; ++z)
	{
		for (std::int32_t x = 0; x < Chunk::Extent; ++x)
		{
			for (std::int32_t y = 0; y < Chunk::Extent; ++y)
			{
				(void)builder.set({x, y, z}, reader.get<Voxel::Cell>());
			}
		}
	}
	chunk = std::move(builder).build();
	return reader;
}
