#include "erelia/core/chunk_protocol.hpp"

#include <algorithm>
#include <atomic>
#include <cstddef>
#include <cstdint>
#include <limits>
#include <type_traits>
#include <unordered_set>

#include <exception.hpp>

namespace
{
	constexpr std::size_t MaximumCoordinateCount = 1024u;

	static_assert(std::is_trivially_copyable_v<Chunk::Coordinate>);
	static_assert(sizeof(Chunk::Coordinate) == 3 * sizeof(std::int32_t));

	std::atomic<spk::Message::RequestID> NextRequestID = 1u;

	[[nodiscard]] spk::Message::Type requestMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(Networking::MessageType::ChunkRequest);
	}

	[[nodiscard]] spk::Message::RequestID generateRequestID()
	{
		auto current = NextRequestID.load(std::memory_order_relaxed);

		while (true)
		{
			if (current == 0u || current == std::numeric_limits<spk::Message::RequestID>::max())
			{
				throw spk::Exception("Chunk::Protocol::Request RequestID sequence is exhausted");
			}

			if (NextRequestID.compare_exchange_weak(
					current,
					current + 1u,
					std::memory_order_relaxed,
					std::memory_order_relaxed))
			{
				return current;
			}
		}
	}

	void validateHeader(const spk::Message &message)
	{
		if (message.type() != requestMessageType())
		{
			throw spk::Exception("Chunk::Protocol::Request has an invalid Message type");
		}
		if (message.requestID() == 0u)
		{
			throw spk::Exception("Chunk::Protocol::Request requires a non-zero RequestID");
		}
	}
}

Chunk::Protocol::Request::Request() :
	spk::Message(requestMessageType())
{
	setRequestID(generateRequestID());
}

Chunk::Protocol::Request::Request(const spk::Message &message) :
	spk::Message(message)
{
	_decode();
}

void Chunk::Protocol::Request::_decode()
{
	validateHeader(*this);

	if (empty())
	{
		throw spk::Exception("Chunk::Protocol::Request payload cannot be empty");
	}
	if (size() % sizeof(Coordinate) != 0u)
	{
		throw spk::Exception("Chunk::Protocol::Request payload is not coordinate-aligned");
	}

	const std::size_t coordinateCount = size() / sizeof(Coordinate);
	if (coordinateCount > MaximumCoordinateCount)
	{
		throw spk::Exception("Chunk::Protocol::Request exceeds the 1024-coordinate limit");
	}

	std::vector<Coordinate> coordinates;
	std::vector<Coordinate> duplicateCoordinates;
	coordinates.reserve(coordinateCount);
	duplicateCoordinates.reserve(coordinateCount);

	std::unordered_set<Coordinate> seen;
	std::unordered_set<Coordinate> duplicated;

	for (std::size_t index = 0; index < coordinateCount; ++index)
	{
		const Coordinate coordinate = readAt<Coordinate>(index * sizeof(Coordinate));

		if (seen.insert(coordinate).second)
		{
			coordinates.push_back(coordinate);
		}
		else if (duplicated.insert(coordinate).second)
		{
			duplicateCoordinates.push_back(coordinate);
		}
	}

	_coordinates = std::move(coordinates);
	_duplicateCoordinates = std::move(duplicateCoordinates);
}

void Chunk::Protocol::Request::add(const Coordinate &coordinate)
{
	validateHeader(*this);

	if (size() % sizeof(Coordinate) != 0u)
	{
		throw spk::Exception("Chunk::Protocol::Request payload is not coordinate-aligned");
	}

	const std::size_t coordinateCount = size() / sizeof(Coordinate);
	if (coordinateCount >= MaximumCoordinateCount)
	{
		throw spk::Exception("Chunk::Protocol::Request cannot contain more than 1024 coordinates");
	}

	append(coordinate);

	const auto existing = std::ranges::find(_coordinates, coordinate);
	if (existing == _coordinates.end())
	{
		_coordinates.push_back(coordinate);
	}
	else if (std::ranges::find(_duplicateCoordinates, coordinate) == _duplicateCoordinates.end())
	{
		_duplicateCoordinates.push_back(coordinate);
	}
}

const std::vector<Chunk::Coordinate> &Chunk::Protocol::Request::coordinates() const noexcept
{
	return _coordinates;
}

const std::vector<Chunk::Coordinate> &Chunk::Protocol::Request::duplicateCoordinates() const noexcept
{
	return _duplicateCoordinates;
}
