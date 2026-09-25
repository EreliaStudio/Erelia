#pragma once

#include <cstdint>
#include <set>
#include <vector>

#include <network/message.hpp>

#include "erelia/core/chunk.hpp"

class Chunk::Protocol::Response final : public spk::Message
{
public:
	enum class State : std::uint8_t
	{
		Success = 0,
		Rejected = 1,
		Unavailable = 2
	};

	class Builder final
	{
	private:
		struct SuccessEntry final
		{
			Coordinate coordinate;
			Chunk chunk;
		};

		spk::Message::RequestID _requestID;
		std::vector<SuccessEntry> _successEntries;
		std::vector<Coordinate> _rejectedCoordinates;
		std::vector<Coordinate> _unavailableCoordinates;
		std::set<Coordinate> _coordinates;

		void _insertCoordinate(const Coordinate &coordinate);

	public:
		explicit Builder(spk::Message::RequestID requestID);

		void addSuccess(const Coordinate &coordinate, const Chunk &chunk);
		void addRejected(const Coordinate &coordinate);
		void addUnavailable(const Coordinate &coordinate);

		[[nodiscard]] Response build() &&;
	};

private:
	explicit Response(spk::Message::RequestID requestID);

	void _validate() const;

public:
	explicit Response(spk::Message message);

	[[nodiscard]] std::uint32_t successOffset() const;
	[[nodiscard]] std::uint32_t rejectedOffset() const;
	[[nodiscard]] std::uint32_t unavailableOffset() const;
};
