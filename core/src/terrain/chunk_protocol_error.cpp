#include "erelia/core/chunk_protocol.hpp"

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <type_traits>

#include <exception.hpp>

namespace
{
	constexpr std::size_t SerializedCodeSize = sizeof(std::uint8_t);
	constexpr std::size_t SerializedEntrySize = SerializedCodeSize + sizeof(Chunk::Coordinate);

	static_assert(std::is_trivially_copyable_v<Chunk::Coordinate>);
	static_assert(sizeof(Chunk::Coordinate) == 3 * sizeof(std::int32_t));

	[[nodiscard]] spk::Message::Type errorMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(Networking::MessageType::ChunkError);
	}

	[[nodiscard]] bool validCode(std::uint8_t code) noexcept
	{
		return code == static_cast<std::uint8_t>(Chunk::Protocol::Error::Code::DuplicateCoordinate);
	}

	void validateHeader(const spk::Message &message)
	{
		if (message.type() != errorMessageType())
		{
			throw spk::Exception("Chunk::Protocol::Error has an invalid Message type");
		}
		if (message.requestID() == 0u)
		{
			throw spk::Exception("Chunk::Protocol::Error requires a non-zero RequestID");
		}
	}
}

Chunk::Protocol::Error::Error(spk::Message::RequestID requestID) :
	spk::Message(errorMessageType())
{
	if (requestID == 0u)
	{
		throw spk::Exception("Chunk::Protocol::Error requires a non-zero RequestID");
	}

	setRequestID(requestID);
}

Chunk::Protocol::Error::Error(const spk::Message &message) :
	spk::Message(message)
{
	_decode();
}

void Chunk::Protocol::Error::_decode()
{
	validateHeader(*this);

	if (size() % SerializedEntrySize != 0u)
	{
		throw spk::Exception("Chunk::Protocol::Error payload is not entry-aligned");
	}

	std::vector<Entry> entries;
	entries.reserve(size() / SerializedEntrySize);

	Coordinate previous{};
	bool hasPrevious = false;

	for (std::size_t offset = 0; offset < size(); offset += SerializedEntrySize)
	{
		const auto rawCode = readAt<std::uint8_t>(offset);
		if (!validCode(rawCode))
		{
			throw spk::Exception("Chunk::Protocol::Error contains an unknown error code");
		}

		const Coordinate coordinate =
			readAt<Coordinate>(offset + SerializedCodeSize);

		if (hasPrevious && !(previous < coordinate))
		{
			throw spk::Exception(
				"Chunk::Protocol::Error coordinates must be unique and sorted X/Y/Z");
		}

		entries.push_back({static_cast<Code>(rawCode), coordinate});
		previous = coordinate;
		hasPrevious = true;
	}

	_entries = std::move(entries);
}

void Chunk::Protocol::Error::_encode()
{
	std::ranges::sort(
		_entries,
		{},
		&Entry::coordinate);

	clear();
	setType(errorMessageType());

	for (const Entry &entry : _entries)
	{
		const auto code = static_cast<std::uint8_t>(entry.code);
		append(code);
		append(entry.coordinate);
	}
}

void Chunk::Protocol::Error::add(Code code, const Coordinate &coordinate)
{
	validateHeader(*this);

	const auto rawCode = static_cast<std::uint8_t>(code);
	if (!validCode(rawCode))
	{
		throw spk::Exception("Chunk::Protocol::Error cannot encode an unknown error code");
	}

	if (std::ranges::find(_entries, coordinate, &Entry::coordinate) != _entries.end())
	{
		throw spk::Exception("Chunk::Protocol::Error cannot encode a duplicate coordinate");
	}

	_entries.push_back({code, coordinate});
	_encode();
}

const std::vector<Chunk::Protocol::Error::Entry> &Chunk::Protocol::Error::entries() const noexcept
{
	return _entries;
}
