#pragma once

#include <cstddef>
#include <cstdint>
#include <set>
#include <string>
#include <vector>

#include <network/message.hpp>

#include "erelia/core/chunk.hpp"

class Chunk::Protocol::Response final : public spk::Message
{
public:
	struct Success final
	{
		Coordinate coordinate;
		Chunk chunk;
	};

	struct Failure final
	{
		enum class Code : std::uint8_t
		{
			AcquisitionFailed = 0
		};

		Coordinate coordinate;
		Code code;
		std::string message;
	};

	class Builder final
	{
	private:
		spk::Message::RequestID _requestID;
		std::vector<Success> _successes;
		std::vector<Failure> _failures;
		std::set<Coordinate> _coordinates;

		void _insertCoordinate(const Coordinate &coordinate);

	public:
		explicit Builder(spk::Message::RequestID requestID);

		void addSuccess(
			const Coordinate &coordinate,
			const Chunk &chunk);
		void addFailure(
			const Coordinate &coordinate,
			Failure::Code code,
			std::string message);

		[[nodiscard]] Response build() &&;
	};

private:
	explicit Response(spk::Message::RequestID requestID);

	void _validate() const;

public:
	explicit Response(spk::Message message);

	[[nodiscard]] std::uint32_t failureOffset() const;
	[[nodiscard]] std::size_t successCount() const;
	[[nodiscard]] Success success(std::size_t index) const;
	[[nodiscard]] std::size_t failureCount() const;
	[[nodiscard]] Failure failure(std::size_t index) const;
};
