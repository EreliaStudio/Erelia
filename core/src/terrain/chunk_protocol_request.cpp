#include "erelia/core/chunk_protocol_request.hpp"\n#include "erelia/core/networking/message_type.hpp"

#include <algorithm>
#include <atomic>
#include <cstddef>
#include <cstdint>
#include <limits>
#include <set>
#include <type_traits>
#include <utility>

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

Chunk::Protocol::Request::Request(spk::Message::RequestID requestID) :
	spk::Message(requestMessageType())
{
	if (requestID == 0u)
	{
		throw spk::Exception("Chunk::Protocol::Request requires a non-zero RequestID");
	}

	setRequestID(requestID);
}

Chunk::Protocol::Request::Request(spk::Message message) :
	spk::Message(std::move(message))
{
	_validate();
}

void Chunk::Protocol::Request::_validate() const
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
	if (coordinateCount() > MaximumCoordinateCount)
	{
		throw spk::Exception("Chunk::Protocol::Request exceeds the 1024-coordinate limit");
	}
}

void Chunk::Protocol::Request::Builder::add(const Coordinate &coordinate)
{
	if (_coordinates.size() >= MaximumCoordinateCount)
	{
		throw spk::Exception("Chunk::Protocol::Request cannot contain more than 1024 coordinates");
	}

#ifndef NDEBUG
	if (std::ranges::find(_coordinates, coordinate) != _coordinates.end())
	{
		throw spk::Exception("Chunk::Protocol::Request Builder contains a duplicate coordinate");
	}
#endif

	_coordinates.push_back(coordinate);
}

Chunk::Protocol::Request Chunk::Protocol::Request::Builder::build() &&
{
	if (_coordinates.empty())
	{
		throw spk::Exception("Chunk::Protocol::Request cannot be built without coordinates");
	}

	Request result(generateRequestID());
	const std::size_t payloadSize = _coordinates.size() * sizeof(Coordinate);

	result.resize(payloadSize);
	result.edit(0u, _coordinates.data(), payloadSize);

	return result;
}

std::size_t Chunk::Protocol::Request::coordinateCount() const noexcept
{
	return size() / sizeof(Coordinate);
}

Chunk::Coordinate Chunk::Protocol::Request::coordinate(std::size_t index) const
{
	if (index >= coordinateCount())
	{
		throw spk::Exception("Chunk::Protocol::Request coordinate index is outside the payload");
	}

	return readAt<Coordinate>(index * sizeof(Coordinate));
}

std::set<Chunk::Coordinate> Chunk::Protocol::Request::duplicateCoordinates() const
{
	std::set<Coordinate> seen;
	std::set<Coordinate> duplicates;

	for (std::size_t index = 0; index < coordinateCount(); ++index)
	{
		const Coordinate current = coordinate(index);
		if (!seen.insert(current).second)
		{
			duplicates.insert(current);
		}
	}

	return duplicates;
}
