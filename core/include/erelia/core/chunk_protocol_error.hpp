#pragma once

#include <cstddef>
#include <cstdint>
#include <vector>

#include <network/message.hpp>

#include "erelia/core/chunk.hpp"

class Chunk::Protocol::Error final : public spk::Message
{
public:
	enum class Code : std::uint8_t
	{
		DuplicateCoordinate = 0
	};

	struct Entry final
	{
		Code code;
		Coordinate coordinate;

		[[nodiscard]] bool operator==(const Entry &) const = default;
	};

	class Builder final
	{
	private:
		spk::Message::RequestID _requestID;
		std::vector<Entry> _entries;

	public:
		explicit Builder(spk::Message::RequestID requestID);

		void add(Code code, const Coordinate &coordinate);
		[[nodiscard]] Error build() &&;
	};

private:
	explicit Error(spk::Message::RequestID requestID);

	void _validate() const;

public:
	explicit Error(spk::Message message);

	[[nodiscard]] std::size_t entryCount() const noexcept;
	[[nodiscard]] Entry entry(std::size_t index) const;
};
