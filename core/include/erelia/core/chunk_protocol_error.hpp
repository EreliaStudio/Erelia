#pragma once

#include <cstddef>
#include <cstdint>
#include <string>
#include <vector>

#include "erelia/core/chunk.hpp"
#include "erelia/core/networking/diagnostic.hpp"

class Chunk::Protocol::Error final : public Networking::Diagnostic
{
public:
	class Builder final
	{
	private:
		spk::Message::RequestID _requestID;
		Networking::Diagnostic::Severity _severity;
		std::string _message;
		std::vector<Coordinate> _coordinates;

	public:
		Builder(
			spk::Message::RequestID requestID,
			Networking::Diagnostic::Severity severity,
			std::string message);

		void add(const Coordinate &coordinate);
		[[nodiscard]] Error build() &&;
	};

private:
	explicit Error(spk::Message::RequestID requestID);

	void _validate() const;

public:
	explicit Error(spk::Message message);

	[[nodiscard]] std::size_t coordinateCount() const;
	[[nodiscard]] Coordinate coordinate(std::size_t index) const;
};
