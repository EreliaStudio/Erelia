#include "erelia/core/chunk_protocol_response.hpp"
#include "erelia/core/networking/message_type.hpp"

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <limits>
#include <set>
#include <type_traits>
#include <utility>

#include <exception.hpp>

namespace
{
	constexpr std::size_t SummaryFieldCount = 3u;
	constexpr std::size_t SummarySize = SummaryFieldCount * sizeof(std::uint32_t);
	constexpr std::size_t SerializedStateSize = sizeof(std::uint8_t);
	constexpr std::size_t ChunkCellCount =
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent);
	constexpr std::size_t ChunkCellBytes = ChunkCellCount * sizeof(Voxel::Cell);
	constexpr std::size_t CoordinateAndStateSize =
		sizeof(Chunk::Coordinate) + SerializedStateSize;
	constexpr std::size_t SuccessEntrySize = CoordinateAndStateSize + ChunkCellBytes;
	constexpr std::size_t ResultEntrySize = CoordinateAndStateSize;

	static_assert(std::is_trivially_copyable_v<Chunk::Coordinate>);
	static_assert(sizeof(Chunk::Coordinate) == 3 * sizeof(std::int32_t));
	static_assert(std::is_trivially_copyable_v<Voxel::Cell>);
	static_assert(sizeof(Voxel::Cell) == sizeof(Voxel::Cell::PackedType));
	static_assert(ChunkCellCount == 4096u);

	[[nodiscard]] spk::Message::Type responseMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(Networking::MessageType::ChunkResponse);
	}

	[[nodiscard]] bool validState(std::uint8_t state) noexcept
	{
		return state == static_cast<std::uint8_t>(Chunk::Protocol::Response::State::Success) || state == static_cast<std::uint8_t>(Chunk::Protocol::Response::State::Rejected) || state == static_cast<std::uint8_t>(Chunk::Protocol::Response::State::Unavailable);
	}

	void validateHeader(const spk::Message &message)
	{
		if (message.type() != responseMessageType())
		{
			throw spk::Exception("Chunk::Protocol::Response has an invalid Message type");
		}
		if (message.requestID() == 0u)
		{
			throw spk::Exception("Chunk::Protocol::Response requires a non-zero RequestID");
		}
	}

	void validateOrderedCoordinate(
		const Chunk::Coordinate &coordinate,
		Chunk::Coordinate &previous,
		bool &hasPrevious,
		std::set<Chunk::Coordinate> &seen)
	{
		if (hasPrevious && !(previous < coordinate))
		{
			throw spk::Exception(
				"Chunk::Protocol::Response group coordinates must be sorted X/Y/Z");
		}
		if (!seen.insert(coordinate).second)
		{
			throw spk::Exception("Chunk::Protocol::Response contains a duplicate coordinate");
		}

		previous = coordinate;
		hasPrevious = true;
	}

	[[nodiscard]] std::size_t checkedOffset(
		std::size_t start,
		std::size_t count,
		std::size_t entrySize)
	{
		constexpr std::size_t Maximum =
			std::numeric_limits<std::uint32_t>::max();

		if (count > (Maximum - start) / entrySize)
		{
			throw spk::Exception("Chunk::Protocol::Response payload exceeds uint32 offsets");
		}

		return start + count * entrySize;
	}

	void validateState(
		std::uint8_t state,
		Chunk::Protocol::Response::State expected)
	{
		if (!validState(state))
		{
			throw spk::Exception("Chunk::Protocol::Response contains an unknown result state");
		}
		if (state != static_cast<std::uint8_t>(expected))
		{
			throw spk::Exception("Chunk::Protocol::Response result state does not match its group");
		}
	}
}

Chunk::Protocol::Response::Response(spk::Message::RequestID requestID) :
	spk::Message(responseMessageType())
{
	if (requestID == 0u)
	{
		throw spk::Exception("Chunk::Protocol::Response requires a non-zero RequestID");
	}

	setRequestID(requestID);
}

Chunk::Protocol::Response::Response(spk::Message message) :
	spk::Message(std::move(message))
{
	_validate();
}

void Chunk::Protocol::Response::_validate() const
{
	validateHeader(*this);

	if (size() < SummarySize)
	{
		throw spk::Exception("Chunk::Protocol::Response is missing its 12-byte summary");
	}

	const std::size_t successOffsetValue = successOffset();
	const std::size_t rejectedOffsetValue = rejectedOffset();
	const std::size_t unavailableOffsetValue = unavailableOffset();

	if (successOffsetValue != SummarySize)
	{
		throw spk::Exception("Chunk::Protocol::Response successOffset must equal 12");
	}
	if (
		rejectedOffsetValue < successOffsetValue ||
		unavailableOffsetValue < rejectedOffsetValue ||
		unavailableOffsetValue > size())
	{
		throw spk::Exception("Chunk::Protocol::Response offsets are outside canonical bounds");
	}

	const std::size_t successBytes = rejectedOffsetValue - successOffsetValue;
	const std::size_t rejectedBytes = unavailableOffsetValue - rejectedOffsetValue;
	const std::size_t unavailableBytes = size() - unavailableOffsetValue;

	if (successBytes % SuccessEntrySize != 0u)
	{
		throw spk::Exception("Chunk::Protocol::Response Success range is not entry-aligned");
	}
	if (rejectedBytes % ResultEntrySize != 0u)
	{
		throw spk::Exception("Chunk::Protocol::Response Rejected range is not entry-aligned");
	}
	if (unavailableBytes % ResultEntrySize != 0u)
	{
		throw spk::Exception("Chunk::Protocol::Response Unavailable range is not entry-aligned");
	}

	std::set<Coordinate> seen;
	Coordinate previous{};
	bool hasPrevious = false;

	for (std::size_t offset = successOffsetValue; offset < rejectedOffsetValue; offset += SuccessEntrySize)
	{
		const Coordinate current = readAt<Coordinate>(offset);
		const auto state = readAt<std::uint8_t>(offset + sizeof(Coordinate));
		validateState(state, State::Success);
		validateOrderedCoordinate(current, previous, hasPrevious, seen);
	}

	previous = {};
	hasPrevious = false;
	for (std::size_t offset = rejectedOffsetValue; offset < unavailableOffsetValue; offset += ResultEntrySize)
	{
		const Coordinate current = readAt<Coordinate>(offset);
		const auto state = readAt<std::uint8_t>(offset + sizeof(Coordinate));
		validateState(state, State::Rejected);
		validateOrderedCoordinate(current, previous, hasPrevious, seen);
	}

	previous = {};
	hasPrevious = false;
	for (std::size_t offset = unavailableOffsetValue; offset < size(); offset += ResultEntrySize)
	{
		const Coordinate current = readAt<Coordinate>(offset);
		const auto state = readAt<std::uint8_t>(offset + sizeof(Coordinate));
		validateState(state, State::Unavailable);
		validateOrderedCoordinate(current, previous, hasPrevious, seen);
	}
}

Chunk::Protocol::Response::Builder::Builder(spk::Message::RequestID requestID) :
	_requestID(requestID)
{
	if (_requestID == 0u)
	{
		throw spk::Exception("Chunk::Protocol::Response requires a non-zero RequestID");
	}
}

void Chunk::Protocol::Response::Builder::_insertCoordinate(const Coordinate &coordinate)
{
	if (!_coordinates.insert(coordinate).second)
	{
		throw spk::Exception("Chunk::Protocol::Response cannot encode a duplicate coordinate");
	}
}

void Chunk::Protocol::Response::Builder::addSuccess(
	const Coordinate &coordinate,
	const Chunk &chunk)
{
	_insertCoordinate(coordinate);
	_successEntries.push_back({coordinate, chunk});
}

void Chunk::Protocol::Response::Builder::addRejected(const Coordinate &coordinate)
{
	_insertCoordinate(coordinate);
	_rejectedCoordinates.push_back(coordinate);
}

void Chunk::Protocol::Response::Builder::addUnavailable(const Coordinate &coordinate)
{
	_insertCoordinate(coordinate);
	_unavailableCoordinates.push_back(coordinate);
}

Chunk::Protocol::Response Chunk::Protocol::Response::Builder::build() &&
{
	std::ranges::sort(
		_successEntries,
		{},
		&SuccessEntry::coordinate);
	std::ranges::sort(_rejectedCoordinates);
	std::ranges::sort(_unavailableCoordinates);

	const std::size_t successOffsetValue = SummarySize;
	const std::size_t rejectedOffsetValue =
		checkedOffset(successOffsetValue, _successEntries.size(), SuccessEntrySize);
	const std::size_t unavailableOffsetValue =
		checkedOffset(rejectedOffsetValue, _rejectedCoordinates.size(), ResultEntrySize);
	const std::size_t finalSize =
		checkedOffset(unavailableOffsetValue, _unavailableCoordinates.size(), ResultEntrySize);

	const auto serializedSuccessOffset =
		static_cast<std::uint32_t>(successOffsetValue);
	const auto serializedRejectedOffset =
		static_cast<std::uint32_t>(rejectedOffsetValue);
	const auto serializedUnavailableOffset =
		static_cast<std::uint32_t>(unavailableOffsetValue);

	Response result(_requestID);
	result.resize(finalSize);
	result.edit(0u, serializedSuccessOffset);
	result.edit(sizeof(std::uint32_t), serializedRejectedOffset);
	result.edit(2u * sizeof(std::uint32_t), serializedUnavailableOffset);

	std::size_t offset = successOffsetValue;
	for (const SuccessEntry &current : _successEntries)
	{
		result.edit(offset, current.coordinate);
		offset += sizeof(Coordinate);

		const auto state = static_cast<std::uint8_t>(State::Success);
		result.edit(offset, state);
		offset += SerializedStateSize;

		const auto cells = current.chunk.cells();
		if (cells.size() != ChunkCellCount)
		{
			throw spk::Exception("Chunk::Protocol::Response Success Chunk must contain exactly 4096 Cells");
		}

		result.edit(offset, cells.data(), ChunkCellBytes);
		offset += ChunkCellBytes;
	}

	for (const Coordinate &coordinate : _rejectedCoordinates)
	{
		result.edit(offset, coordinate);
		offset += sizeof(Coordinate);

		const auto state = static_cast<std::uint8_t>(State::Rejected);
		result.edit(offset, state);
		offset += SerializedStateSize;
	}

	for (const Coordinate &coordinate : _unavailableCoordinates)
	{
		result.edit(offset, coordinate);
		offset += sizeof(Coordinate);

		const auto state = static_cast<std::uint8_t>(State::Unavailable);
		result.edit(offset, state);
		offset += SerializedStateSize;
	}

	return result;
}

std::uint32_t Chunk::Protocol::Response::successOffset() const
{
	return readAt<std::uint32_t>(0u);
}

std::uint32_t Chunk::Protocol::Response::rejectedOffset() const
{
	return readAt<std::uint32_t>(sizeof(std::uint32_t));
}

std::uint32_t Chunk::Protocol::Response::unavailableOffset() const
{
	return readAt<std::uint32_t>(2u * sizeof(std::uint32_t));
}
