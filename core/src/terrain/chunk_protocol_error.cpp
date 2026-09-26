#include "erelia/core/chunk_protocol_error.hpp"
#include "erelia/core/networking/message_type.hpp"

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <limits>
#include <type_traits>
#include <utility>

#include <exception.hpp>

namespace
{
	constexpr std::size_t SerializedCoordinateCountSize =
		sizeof(std::uint32_t);

	static_assert(std::is_trivially_copyable_v<Chunk::Coordinate>);
	static_assert(
		sizeof(Chunk::Coordinate) ==
		3 * sizeof(std::int32_t));

	[[nodiscard]] spk::Message::Type errorMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(
			Networking::MessageType::ChunkError);
	}
}

Chunk::Protocol::Error::Builder::Builder(
	spk::Message::RequestID requestID,
	Networking::Diagnostic::Severity severity,
	std::string message) :
	_requestID(requestID),
	_severity(severity),
	_message(std::move(message))
{
	if (_requestID == 0u)
	{
		throw spk::Exception(
			"Chunk::Protocol::Error requires a non-zero RequestID");
	}
}

void Chunk::Protocol::Error::Builder::add(
	const Coordinate &coordinate)
{
	if (std::ranges::find(_coordinates, coordinate) !=
		_coordinates.end())
	{
		throw spk::Exception(
			"Chunk::Protocol::Error cannot encode a duplicate coordinate");
	}

	_coordinates.push_back(coordinate);
}

Chunk::Protocol::Error Chunk::Protocol::Error::Builder::build() &&
{
	std::ranges::sort(_coordinates);

	auto diagnostic =
		Networking::Diagnostic::Builder(
			_severity,
			std::move(_message),
			_requestID)
			.build();

	if (_coordinates.size() >
		std::numeric_limits<std::uint32_t>::max())
	{
		throw spk::Exception(
			"Chunk::Protocol::Error coordinate count exceeds uint32 capacity");
	}

	Error result(_requestID);
	result << static_cast<const Networking::Diagnostic &>(
		diagnostic);

	const auto coordinateCount =
		static_cast<std::uint32_t>(_coordinates.size());
	result << coordinateCount;
	result.append(
		_coordinates.data(),
		_coordinates.size() * sizeof(Coordinate));

	result._validate();
	return result;
}

Chunk::Protocol::Error::Error(
	spk::Message::RequestID requestID) :
	Networking::Diagnostic(
		errorMessageType(),
		requestID)
{
	if (requestID == 0u)
	{
		throw spk::Exception(
			"Chunk::Protocol::Error requires a non-zero RequestID");
	}
}

Chunk::Protocol::Error::Error(spk::Message message) :
	Networking::Diagnostic(
		std::move(message),
		errorMessageType(),
		true)
{
	_validate();
}

void Chunk::Protocol::Error::_validate() const
{
	if (requestID() == 0u)
	{
		throw spk::Exception(
			"Chunk::Protocol::Error requires a non-zero RequestID");
	}

	const std::size_t prefixSize = diagnosticSize();
	if (size() < prefixSize + SerializedCoordinateCountSize)
	{
		throw spk::Exception(
			"Chunk::Protocol::Error is missing its coordinate count");
	}

	const auto count =
		readAt<std::uint32_t>(prefixSize);
	const std::size_t coordinateBytes =
		size() - prefixSize - SerializedCoordinateCountSize;

	if (count >
		std::numeric_limits<std::size_t>::max() /
			sizeof(Coordinate))
	{
		throw spk::Exception(
			"Chunk::Protocol::Error coordinate block exceeds size_t capacity");
	}

	if (
		static_cast<std::size_t>(count) *
			sizeof(Coordinate) !=
		coordinateBytes)
	{
		throw spk::Exception(
			"Chunk::Protocol::Error coordinate count does not match its payload");
	}

	Coordinate previous{};
	bool hasPrevious = false;
	for (std::size_t index = 0u;
		 index < coordinateCount();
		 ++index)
	{
		const Coordinate current = coordinate(index);
		if (hasPrevious && !(previous < current))
		{
			throw spk::Exception(
				"Chunk::Protocol::Error coordinates must be unique and sorted X/Y/Z");
		}

		previous = current;
		hasPrevious = true;
	}
}

std::size_t Chunk::Protocol::Error::coordinateCount() const
{
	return static_cast<std::size_t>(
		readAt<std::uint32_t>(diagnosticSize()));
}

Chunk::Coordinate Chunk::Protocol::Error::coordinate(
	std::size_t index) const
{
	const std::size_t count = coordinateCount();
	if (index >= count)
	{
		throw spk::Exception(
			"Chunk::Protocol::Error coordinate index is outside the payload");
	}

	return readAt<Coordinate>(
		diagnosticSize() +
		SerializedCoordinateCountSize +
		index * sizeof(Coordinate));
}
