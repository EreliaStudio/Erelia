#include "erelia/core/chunk_protocol_response.hpp"
#include "erelia/core/chunk_builder.hpp"
#include "erelia/core/networking/message_type.hpp"

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <limits>
#include <set>
#include <string>
#include <type_traits>
#include <utility>

#include <exception.hpp>

namespace
{
	constexpr std::size_t SummarySize = sizeof(std::uint32_t);
	constexpr std::size_t SerializedFailureCodeSize =
		sizeof(std::uint8_t);
	constexpr std::size_t SerializedStringLengthSize =
		sizeof(std::uint32_t);
	constexpr std::size_t ChunkCellCount =
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent) *
		static_cast<std::size_t>(Chunk::Extent);
	constexpr std::size_t ChunkCellBytes =
		ChunkCellCount *
		sizeof(Voxel::Cell::PackedType);
	constexpr std::size_t SuccessEntrySize =
		sizeof(Chunk::Coordinate) +
		ChunkCellBytes;
	constexpr std::size_t FailureFixedSize =
		sizeof(Chunk::Coordinate) +
		SerializedFailureCodeSize +
		SerializedStringLengthSize;

	static_assert(std::is_trivially_copyable_v<Chunk::Coordinate>);
	static_assert(
		sizeof(Chunk::Coordinate) ==
		3 * sizeof(std::int32_t));
	static_assert(std::is_trivially_copyable_v<Voxel::Cell>);
	static_assert(
		sizeof(Voxel::Cell) ==
		sizeof(Voxel::Cell::PackedType));
	static_assert(ChunkCellCount == 4096u);

	[[nodiscard]] spk::Message::Type responseMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(
			Networking::MessageType::ChunkResponse);
	}

	[[nodiscard]] bool validFailureCode(
		std::uint8_t code) noexcept
	{
		return code ==
			static_cast<std::uint8_t>(
				Chunk::Protocol::Response::Failure::Code::
					AcquisitionFailed);
	}

	void validateHeader(const spk::Message &message)
	{
		if (message.type() != responseMessageType())
		{
			throw spk::Exception(
				"Chunk::Protocol::Response has an invalid Message type");
		}
		if (message.requestID() == 0u)
		{
			throw spk::Exception(
				"Chunk::Protocol::Response requires a non-zero RequestID");
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
			throw spk::Exception(
				"Chunk::Protocol::Response contains a duplicate coordinate");
		}

		previous = coordinate;
		hasPrevious = true;
	}

	[[nodiscard]] std::size_t checkedAdd(
		std::size_t base,
		std::size_t amount)
	{
		if (
			amount >
			std::numeric_limits<std::size_t>::max() - base)
		{
			throw spk::Exception(
				"Chunk::Protocol::Response payload exceeds size_t capacity");
		}
		return base + amount;
	}

	[[nodiscard]] std::size_t failureEntrySize(
		const Chunk::Protocol::Response::Failure &failure)
	{
		if (
			failure.message.size() >
			std::numeric_limits<std::uint32_t>::max())
		{
			throw spk::Exception(
				"Chunk::Protocol::Response Failure message exceeds uint32 capacity");
		}

		return checkedAdd(
			FailureFixedSize,
			failure.message.size());
	}

	[[nodiscard]] std::size_t nextFailureOffset(
		const spk::Message &message,
		std::size_t offset)
	{
		if (
			offset > message.size() ||
			message.size() - offset < FailureFixedSize)
		{
			throw spk::Exception(
				"Chunk::Protocol::Response Failure entry is truncated");
		}

		const auto messageLength =
			message.readAt<std::uint32_t>(
				offset +
				sizeof(Chunk::Coordinate) +
				SerializedFailureCodeSize);
		const std::size_t entrySize =
			checkedAdd(
				FailureFixedSize,
				static_cast<std::size_t>(messageLength));

		if (entrySize > message.size() - offset)
		{
			throw spk::Exception(
				"Chunk::Protocol::Response Failure message is truncated");
		}
		return offset + entrySize;
	}

	[[nodiscard]] Voxel::Volume::LocalCoordinate
	localCoordinate(std::size_t index) noexcept
	{
		constexpr std::size_t extent =
			static_cast<std::size_t>(Chunk::Extent);
		const auto y =
			static_cast<std::int32_t>(index % extent);
		const auto x =
			static_cast<std::int32_t>(
				(index / extent) % extent);
		const auto z =
			static_cast<std::int32_t>(
				index / (extent * extent));
		return {x, y, z};
	}
}

Chunk::Protocol::Response::Response(
	spk::Message::RequestID requestID) :
	spk::Message(responseMessageType())
{
	if (requestID == 0u)
	{
		throw spk::Exception(
			"Chunk::Protocol::Response requires a non-zero RequestID");
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
		throw spk::Exception(
			"Chunk::Protocol::Response is missing its failure offset");
	}

	const std::size_t failureOffsetValue =
		failureOffset();
	if (
		failureOffsetValue < SummarySize ||
		failureOffsetValue > size())
	{
		throw spk::Exception(
			"Chunk::Protocol::Response failureOffset is outside canonical bounds");
	}

	const std::size_t successBytes =
		failureOffsetValue - SummarySize;
	if (successBytes % SuccessEntrySize != 0u)
	{
		throw spk::Exception(
			"Chunk::Protocol::Response Success range is not entry-aligned");
	}

	std::set<Coordinate> seen;
	Coordinate previous{};
	bool hasPrevious = false;

	for (
		std::size_t offset = SummarySize;
		offset < failureOffsetValue;
		offset += SuccessEntrySize)
	{
		const Coordinate current =
			readAt<Coordinate>(offset);
		validateOrderedCoordinate(
			current,
			previous,
			hasPrevious,
			seen);
	}

	previous = {};
	hasPrevious = false;
	for (
		std::size_t offset = failureOffsetValue;
		offset < size();)
	{
		if (size() - offset < FailureFixedSize)
		{
			throw spk::Exception(
				"Chunk::Protocol::Response Failure entry is truncated");
		}

		const Coordinate current =
			readAt<Coordinate>(offset);
		validateOrderedCoordinate(
			current,
			previous,
			hasPrevious,
			seen);

		const auto rawCode =
			readAt<std::uint8_t>(
				offset + sizeof(Coordinate));
		if (!validFailureCode(rawCode))
		{
			throw spk::Exception(
				"Chunk::Protocol::Response contains an unknown Failure code");
		}

		offset = nextFailureOffset(*this, offset);
	}
}

void Chunk::Protocol::Response::Builder::_insertCoordinate(
	const Coordinate &coordinate)
{
	if (!_coordinates.insert(coordinate).second)
	{
		throw spk::Exception(
			"Chunk::Protocol::Response cannot encode a duplicate coordinate");
	}
}

Chunk::Protocol::Response::Builder::Builder(
	spk::Message::RequestID requestID) :
	_requestID(requestID)
{
	if (_requestID == 0u)
	{
		throw spk::Exception(
			"Chunk::Protocol::Response requires a non-zero RequestID");
	}
}

void Chunk::Protocol::Response::Builder::addSuccess(
	const Coordinate &coordinate,
	const Chunk &chunk)
{
	_insertCoordinate(coordinate);
	_successes.push_back({coordinate, chunk});
}

void Chunk::Protocol::Response::Builder::addFailure(
	const Coordinate &coordinate,
	Failure::Code code,
	std::string message)
{
	if (!validFailureCode(
			static_cast<std::uint8_t>(code)))
	{
		throw spk::Exception(
			"Chunk::Protocol::Response cannot encode an unknown Failure code");
	}

	_insertCoordinate(coordinate);
	_failures.push_back(
		{coordinate, code, std::move(message)});
}

Chunk::Protocol::Response Chunk::Protocol::Response::Builder::build() &&
{
	std::ranges::sort(
		_successes,
		{},
		&Success::coordinate);
	std::ranges::sort(
		_failures,
		{},
		&Failure::coordinate);

	if (
		_successes.size() >
		(std::numeric_limits<std::uint32_t>::max() -
		 SummarySize) /
			SuccessEntrySize)
	{
		throw spk::Exception(
			"Chunk::Protocol::Response Success section exceeds uint32 offsets");
	}

	const std::size_t failureOffsetValue =
		SummarySize +
		_successes.size() * SuccessEntrySize;

	std::size_t finalSize = failureOffsetValue;
	for (const Failure &failure : _failures)
	{
		finalSize = checkedAdd(
			finalSize,
			failureEntrySize(failure));
	}

	Response result(_requestID);
	result.resize(finalSize);
	result.edit(
		0u,
		static_cast<std::uint32_t>(
			failureOffsetValue));

	std::size_t offset = SummarySize;
	for (const Success &current : _successes)
	{
		result.edit(offset, current.coordinate);
		offset += sizeof(Coordinate);

		const auto cells = current.chunk.cells();
		if (cells.size() != ChunkCellCount)
		{
			throw spk::Exception(
				"Chunk::Protocol::Response Success Chunk must contain exactly 4096 Cells");
		}

		result.edit(
			offset,
			cells.data(),
			ChunkCellBytes);
		offset += ChunkCellBytes;
	}

	for (const Failure &current : _failures)
	{
		result.edit(offset, current.coordinate);
		offset += sizeof(Coordinate);

		result.edit(
			offset,
			static_cast<std::uint8_t>(
				current.code));
		offset += SerializedFailureCodeSize;

		const auto messageLength =
			static_cast<std::uint32_t>(
				current.message.size());
		result.edit(offset, messageLength);
		offset += SerializedStringLengthSize;

		result.edit(
			offset,
			current.message.data(),
			current.message.size());
		offset += current.message.size();
	}

	result._validate();
	return result;
}

std::uint32_t Chunk::Protocol::Response::failureOffset() const
{
	return readAt<std::uint32_t>(0u);
}

std::size_t Chunk::Protocol::Response::successCount() const
{
	return (
		static_cast<std::size_t>(failureOffset()) -
		SummarySize) /
		SuccessEntrySize;
}

Chunk::Protocol::Response::Success
Chunk::Protocol::Response::success(
	std::size_t index) const
{
	if (index >= successCount())
	{
		throw spk::Exception(
			"Chunk::Protocol::Response Success index is outside the payload");
	}

	const std::size_t offset =
		SummarySize +
		index * SuccessEntrySize;
	const Coordinate coordinate =
		readAt<Coordinate>(offset);

	Chunk::Builder builder;
	const std::size_t cellOffset =
		offset + sizeof(Coordinate);
	for (
		std::size_t cellIndex = 0u;
		cellIndex < ChunkCellCount;
		++cellIndex)
	{
		const auto packed =
			readAt<Voxel::Cell::PackedType>(
				cellOffset +
				cellIndex *
					sizeof(Voxel::Cell::PackedType));
		(void)builder.set(
			localCoordinate(cellIndex),
			Voxel::Cell(packed));
	}

	return {
		coordinate,
		std::move(builder).build()};
}

std::size_t Chunk::Protocol::Response::failureCount() const
{
	std::size_t count = 0u;
	for (
		std::size_t offset = failureOffset();
		offset < size();
		offset = nextFailureOffset(*this, offset))
	{
		++count;
	}
	return count;
}

Chunk::Protocol::Response::Failure
Chunk::Protocol::Response::failure(
	std::size_t index) const
{
	std::size_t currentIndex = 0u;
	for (
		std::size_t offset = failureOffset();
		offset < size();
		offset = nextFailureOffset(*this, offset))
	{
		if (currentIndex == index)
		{
			const Coordinate coordinate =
				readAt<Coordinate>(offset);
			const auto code =
				static_cast<Failure::Code>(
					readAt<std::uint8_t>(
						offset +
						sizeof(Coordinate)));
			const auto messageLength =
				readAt<std::uint32_t>(
					offset +
					sizeof(Coordinate) +
					SerializedFailureCodeSize);

			std::string message(
				static_cast<std::size_t>(
					messageLength),
				'\0');
			readAt(
				offset +
					FailureFixedSize,
				message.data(),
				message.size());

			return {
				coordinate,
				code,
				std::move(message)};
		}
		++currentIndex;
	}

	throw spk::Exception(
		"Chunk::Protocol::Response Failure index is outside the payload");
}
