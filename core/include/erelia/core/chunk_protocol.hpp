#pragma once

#include <cstdint>
#include <set>
#include <vector>

#include <network/message.hpp>

#include "erelia/core/chunk.hpp"
#include "erelia/core/networking/message_type.hpp"

class Chunk::Protocol final
{
public:
	class Request final : public spk::Message
	{
	private:
		std::set<Coordinate> _coordinates;
		std::set<Coordinate> _duplicateCoordinates;

		void _decode();

	public:
		Request();
		explicit Request(const spk::Message &message);

		[[nodiscard]] bool add(const Coordinate &coordinate);

		[[nodiscard]] const std::set<Coordinate> &coordinates() const noexcept;
		[[nodiscard]] const std::set<Coordinate> &duplicateCoordinates() const noexcept;
	};

	class Error final : public spk::Message
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

	private:
		std::vector<Entry> _entries;

		void _decode();
		void _encode();

	public:
		explicit Error(spk::Message::RequestID requestID);
		explicit Error(const spk::Message &message);

		void add(Code code, const Coordinate &coordinate);

		[[nodiscard]] const std::vector<Entry> &entries() const noexcept;
	};

	class Response final : public spk::Message
	{
	public:
		enum class State : std::uint8_t
		{
			Success = 0,
			Rejected = 1,
			Unavailable = 2
		};

	private:
		struct SuccessEntry final
		{
			Coordinate coordinate;
			Chunk chunk;
		};

		std::vector<SuccessEntry> _successEntries;
		std::vector<Coordinate> _rejectedCoordinates;
		std::vector<Coordinate> _unavailableCoordinates;
		std::uint32_t _successOffset = 0;
		std::uint32_t _rejectedOffset = 0;
		std::uint32_t _unavailableOffset = 0;

		void _decode();
		void _encode();
		[[nodiscard]] bool _contains(const Coordinate &coordinate) const noexcept;

	public:
		explicit Response(spk::Message::RequestID requestID);
		explicit Response(const spk::Message &message);

		void addSuccess(const Coordinate &coordinate, const Chunk &chunk);
		void addRejected(const Coordinate &coordinate);
		void addUnavailable(const Coordinate &coordinate);

		[[nodiscard]] std::uint32_t successOffset() const noexcept;
		[[nodiscard]] std::uint32_t rejectedOffset() const noexcept;
		[[nodiscard]] std::uint32_t unavailableOffset() const noexcept;
	};
};
