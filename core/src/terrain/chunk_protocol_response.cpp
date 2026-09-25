#include "erelia/core/chunk_protocol.hpp"

#include "erelia/core/chunk_builder.hpp"

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <limits>
#include <type_traits>
#include <unordered_set>
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
	constexpr std::size_t ChunkCellBytes = ChunkCellCount * sizeof(Voxel::Cell::PackedType);
	constexpr std::size_t CoordinateAndStateSize =
		sizeof(Chunk::Coordinate) + SerializedStateSize;
	constexpr std::size_t SuccessEntrySize = CoordinateAndStateSize + ChunkCellBytes;
	constexpr std::size_t ResultEntrySize = CoordinateAndStateSize;

	static_assert(std::is_trivially_copyable_v<Chunk::Coordinate>);
	static_assert(sizeof(Chunk::Coordinate) == 3 * sizeof(std::int32_t));
	static_assert(std::is_trivially_copyable_v<Voxel::Cell::PackedType>);
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
		std::unordered_set<Chunk::Coordinate> &seen)
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
		constexpr auto Maximum = std::numeric_limits<std::uint32_t>::max();

		if (count > (Maximum - start) / entrySize)
		{
			throw spk::Exception("Chunk::Protocol::Response payload exceeds uint32 offsets");
		}

		return start + count * entrySize;
	}

	[[nodiscard]] Chunk decodeChunk(
		const spk::Message &message,
		std::size_t cellOffset)
	{
		Chunk::Builder builder;

		for (std::size_t index = 0; index < ChunkCellCount; ++index)
		{
			const auto packed = message.readAt<Voxel::Cell::PackedType>(
				cellOffset + index * sizeof(Voxel::Cell::PackedType));

			if (packed == Voxel::Cell::Empty.packed())
			{
				continue;
			}

			const auto y = static_cast<std::int32_t>(index % Chunk::Extent);
			const auto x = static_cast<std::int32_t>(
				(index / Chunk::Extent) % Chunk::Extent);
			const auto z = static_cast<std::int32_t>(
				index / (Chunk::Extent * Chunk::Extent));

			(void)builder.set({x, y, z}, Voxel::Cell(packed));
		}

		return std::move(builder).build();
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
	_encode();
}

Chunk::Protocol::Response::Response(const spk::Message &message) :
	spk::Message(message)
{
	_decode();
}

void Chunk::Protocol::Response::_decode()
{
	validateHeader(*this);

	if (size() < SummarySize)
	{
		throw spk::Exception("Chunk::Protocol::Response is missing its 12-byte summary");
	}

	const auto successOffsetValue = readAt<std::uint32_t>(0u);
	const auto rejectedOffsetValue = readAt<std::uint32_t>(sizeof(std::uint32_t));
	const auto unavailableOffsetValue =
		readAt<std::uint32_t>(2u * sizeof(std::uint32_t));

	if (successOffsetValue != SummarySize)
	{
		throw spk::Exception("Chunk::Protocol::Response successOffset must equal 12");
	}

	const std::size_t successOffset = successOffsetValue;
	const std::size_t rejectedOffset = rejectedOffsetValue;
	const std::size_t unavailableOffset = unavailableOffsetValue;

	if (
		rejectedOffset < successOffset ||
		unavailableOffset < rejectedOffset ||
		unavailableOffset > size())
	{
		throw spk::Exception("Chunk::Protocol::Response offsets are outside canonical bounds");
	}

	const std::size_t successBytes = rejectedOffset - successOffset;
	const std::size_t rejectedBytes = unavailableOffset - rejectedOffset;
	const std::size_t unavailableBytes = size() - unavailableOffset;

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

	std::vector<SuccessEntry> successEntries;
	std::vector<Coordinate> rejectedCoordinates;
	std::vector<Coordinate> unavailableCoordinates;
	successEntries.reserve(successBytes / SuccessEntrySize);
	rejectedCoordinates.reserve(rejectedBytes / ResultEntrySize);
	unavailableCoordinates.reserve(unavailableBytes / ResultEntrySize);

	std::unordered_set<Coordinate> seen;

	Coordinate previous{};
	bool hasPrevious = false;
	for (std::size_t offset = successOffset; offset < rejectedOffset; offset += SuccessEntrySize)
	{
		const Coordinate coordinate = readAt<Coordinate>(offset);
		const auto state = readAt<std::uint8_t>(offset + sizeof(Coordinate));

		if (!validState(state))
		{
			throw spk::Exception("Chunk::Protocol::Response contains an unknown result state");
		}
		if (state != static_cast<std::uint8_t>(State::Success))
		{
			throw spk::Exception(
				"Chunk::Protocol::Response Success range contains another result state");
		}

		validateOrderedCoordinate(coordinate, previous, hasPrevious, seen);
		successEntries.push_back({coordinate, decodeChunk(*this, offset + CoordinateAndStateSize)});
	}

	previous = {};
	hasPrevious = false;
	for (std::size_t offset = rejectedOffset; offset < unavailableOffset; offset += ResultEntrySize)
	{
		const Coordinate coordinate = readAt<Coordinate>(offset);
		const auto state = readAt<std::uint8_t>(offset + sizeof(Coordinate));

		if (!validState(state))
		{
			throw spk::Exception("Chunk::Protocol::Response contains an unknown result state");
		}
		if (state != static_cast<std::uint8_t>(State::Rejected))
		{
			throw spk::Exception(
				"Chunk::Protocol::Response Rejected range contains another result state");
		}

		validateOrderedCoordinate(coordinate, previous, hasPrevious, seen);
		rejectedCoordinates.push_back(coordinate);
	}

	previous = {};
	hasPrevious = false;
	for (std::size_t offset = unavailableOffset; offset < size(); offset += ResultEntrySize)
	{
		const Coordinate coordinate = readAt<Coordinate>(offset);
		const auto state = readAt<std::uint8_t>(offset + sizeof(Coordinate));

		if (!validState(state))
		{
			throw spk::Exception("Chunk::Protocol::Response contains an unknown result state");
		}
		if (state != static_cast<std::uint8_t>(State::Unavailable))
		{
			throw spk::Exception(
				"Chunk::Protocol::Response Unavailable range contains another result state");
		}

		validateOrderedCoordinate(coordinate, previous, hasPrevious, seen);
		unavailableCoordinates.push_back(coordinate);
	}

	_successEntries = std::move(successEntries);
	_rejectedCoordinates = std::move(rejectedCoordinates);
	_unavailableCoordinates = std::move(unavailableCoordinates);
	_successOffset = successOffsetValue;
	_rejectedOffset = rejectedOffsetValue;
	_unavailableOffset = unavailableOffsetValue;
}

void Chunk::Protocol::Response::_encode()
{
	std::ranges::sort(
		_successEntries,
		{},
		&SuccessEntry::coordinate);
	std::ranges::sort(_rejectedCoordinates);
	std::ranges::sort(_unavailableCoordinates);

	const std::size_t successOffset = SummarySize;
	const std::size_t rejectedOffset =
		checkedOffset(successOffset, _successEntries.size(), SuccessEntrySize);
	const std::size_t unavailableOffset =
		checkedOffset(rejectedOffset, _rejectedCoordinates.size(), ResultEntrySize);
	(void)checkedOffset(unavailableOffset, _unavailableCoordinates.size(), ResultEntrySize);

	_successOffset = static_cast<std::uint32_t>(successOffset);
	_rejectedOffset = static_cast<std::uint32_t>(rejectedOffset);
	_unavailableOffset = static_cast<std::uint32_t>(unavailableOffset);

	clear();
	setType(responseMessageType());

	append(_successOffset);
	append(_rejectedOffset);
	append(_unavailableOffset);

	for (const SuccessEntry &entry : _successEntries)
	{
		const auto cells = entry.chunk.cells();
		if (cells.size() != ChunkCellCount)
		{
			throw spk::Exception(
				"Chunk::Protocol::Response Success requires a complete 4096-Cell Chunk");
		}

		append(entry.coordinate);
		append(static_cast<std::uint8_t>(State::Success));
		for (const Voxel::Cell &cell : cells)
		{
			append(cell.packed());
		}
	}

	for (const Coordinate &coordinate : _rejectedCoordinates)
	{
		append(coordinate);
		append(static_cast<std::uint8_t>(State::Rejected));
	}

	for (const Coordinate &coordinate : _unavailableCoordinates)
	{
		append(coordinate);
		append(static_cast<std::uint8_t>(State::Unavailable));
	}
}

bool Chunk::Protocol::Response::_contains(const Coordinate &coordinate) const noexcept
{
	if (
		std::ranges::find(_successEntries, coordinate, &SuccessEntry::coordinate) !=
		_successEntries.end())
	{
		return true;
	}
	if (std::ranges::find(_rejectedCoordinates, coordinate) != _rejectedCoordinates.end())
	{
		return true;
	}

	return std::ranges::find(_unavailableCoordinates, coordinate) != _unavailableCoordinates.end();
}

void Chunk::Protocol::Response::addSuccess(
	const Coordinate &coordinate,
	const Chunk &chunk)
{
	validateHeader(*this);

	if (_contains(coordinate))
	{
		throw spk::Exception("Chunk::Protocol::Response cannot encode a duplicate coordinate");
	}

	_successEntries.push_back({coordinate, chunk});
	_encode();
}

void Chunk::Protocol::Response::addRejected(const Coordinate &coordinate)
{
	validateHeader(*this);

	if (_contains(coordinate))
	{
		throw spk::Exception("Chunk::Protocol::Response cannot encode a duplicate coordinate");
	}

	_rejectedCoordinates.push_back(coordinate);
	_encode();
}

void Chunk::Protocol::Response::addUnavailable(const Coordinate &coordinate)
{
	validateHeader(*this);

	if (_contains(coordinate))
	{
		throw spk::Exception("Chunk::Protocol::Response cannot encode a duplicate coordinate");
	}

	_unavailableCoordinates.push_back(coordinate);
	_encode();
}

std::uint32_t Chunk::Protocol::Response::successOffset() const noexcept
{
	return _successOffset;
}

std::uint32_t Chunk::Protocol::Response::rejectedOffset() const noexcept
{
	return _rejectedOffset;
}

std::uint32_t Chunk::Protocol::Response::unavailableOffset() const noexcept
{
	return _unavailableOffset;
}
