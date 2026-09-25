#pragma once

#include <cstddef>
#include <set>
#include <vector>

#include <network/message.hpp>

#include "erelia/core/chunk.hpp"

class Chunk::Protocol::Request final : public spk::Message
{
public:
	class Builder final
	{
	private:
		std::vector<Coordinate> _coordinates;

	public:
		void add(const Coordinate &coordinate);
		[[nodiscard]] Request build() &&;
	};

private:
	explicit Request(spk::Message::RequestID requestID);

	void _validate() const;

public:
	explicit Request(spk::Message message);

	[[nodiscard]] std::size_t coordinateCount() const noexcept;
	[[nodiscard]] Coordinate coordinate(std::size_t index) const;
	[[nodiscard]] std::set<Coordinate> duplicateCoordinates() const;
};
