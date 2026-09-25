#include "erelia/core/chunk_protocol_error.hpp"\n#include "erelia/core/networking/message_type.hpp"

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <type_traits>
#include <utility>

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

Chunk::Protocol::Error::Error(spk::Message message) :
	spk::Message(std::move(message))
{
	_validate();
}

void Chunk::Protocol::Error::_validate() const
{
	validateHeader(*this);

	if (size() % SerializedEntrySize != 0u)
	{
		throw spk::Exception("Chunk::Protocol::Error payload is not entry-aligned");
	}

	Coordinate previous{};
	bool hasPrevious = false;

	for (std::size_t index = 0; index < entryCount(); ++index)
	{
		const Entry current = entry(index);

		if (hasPrevious && !(previous < current.coordinate))
		{
			throw spk::Exception(
				"Chunk::Protocol::Error coordinates must be unique and sorted X/Y/Z");
		}

		previous = current.coordinate;
		hasPrevious = true;
	}
}

Chunk::Protocol::Error::Builder::Builder(spk::Message::RequestID requestID) :
	_requestID(requestID)
{
	if (_requestID == 0u)
	{
		throw spk::Exception("Chunk::Protocol::Error requires a non-zero RequestID");
	}
}

void Chunk::Protocol::Error::Builder::add(Code code, const Coordinate &coordinate)
{
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
}

Chunk::Protocol::Error Chunk::Protocol::Error::Builder::build() &&
{
	std::ranges::sort(
		_entries,
		{},
		&Entry::coordinate);

	Error result(_requestID);
	result.resize(_entries.size() * SerializedEntrySize);

	std::size_t offset = 0u;
	for (const Entry &current : _entries)
	{
		const auto code = static_cast<std::uint8_t>(current.code);
		result.edit(offset, code);
		offset += SerializedCodeSize;
		result.edit(offset, current.coordinate);
		offset += sizeof(Coordinate);
	}

	return result;
}

std::size_t Chunk::Protocol::Error::entryCount() const noexcept
{
	return size() / SerializedEntrySize;
}

Chunk::Protocol::Error::Entry Chunk::Protocol::Error::entry(std::size_t index) const
{
	if (index >= entryCount())
	{
		throw spk::Exception("Chunk::Protocol::Error entry index is outside the payload");
	}

	const std::size_t offset = index * SerializedEntrySize;
	const auto rawCode = readAt<std::uint8_t>(offset);
	if (!validCode(rawCode))
	{
		throw spk::Exception("Chunk::Protocol::Error contains an unknown error code");
	}

	return {
		static_cast<Code>(rawCode),
		readAt<Coordinate>(offset + SerializedCodeSize)};
}
