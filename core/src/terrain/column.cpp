#include "erelia/core/column.hpp"
#include <algorithm>
#include <exception.hpp>
#include <limits>
namespace
{
	void validate(const Column &column)
	{
		for (std::size_t index = 1; index < column.chunks.size(); ++index)
		{
			if ((column.chunks[index - 1] < column.chunks[index]) == false)
			{
				throw spk::Exception("Column coordinates must be distinct and ordered");
			}
			if (column.chunks[index].x != column.chunks[0].x || column.chunks[index].z != column.chunks[0].z)
			{
				throw spk::Exception("Column mixes horizontal coordinates");
			}
		}
	}
}
spk::Message::Writer &operator<<(spk::Message::Writer &writer, const Column &column)
{
	validate(column);
	if (column.chunks.size() > std::numeric_limits<std::uint32_t>::max())
	{
		throw spk::Exception("Column is too large");
	}
	writer << static_cast<std::uint32_t>(column.chunks.size());
	for (const auto &coordinate : column.chunks)
	{
		writer << coordinate;
	}
	return writer;
}
const spk::Message::Reader &operator>>(const spk::Message::Reader &reader, Column &column)
{
	const auto count = reader.get<std::uint32_t>();
	if (count > (reader.size() - reader.readOffset()) / sizeof(Chunk::Coordinate))
	{
		throw spk::Exception("Truncated Column");
	}
	Column decoded;
	decoded.chunks.resize(count);
	for (auto &coordinate : decoded.chunks)
	{
		reader >> coordinate;
	}
	validate(decoded);
	column = std::move(decoded);
	return reader;
}
