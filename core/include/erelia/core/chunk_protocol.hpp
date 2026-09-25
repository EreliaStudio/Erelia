#pragma once

#include <cstddef>
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

	class Response final : public spk::Message
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
};
