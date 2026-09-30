#include "erelia/core/networking/diagnostic.hpp"
#include "erelia/core/networking/message_type.hpp"

#include <cstddef>
#include <cstdint>
#include <utility>

#include <exception.hpp>

namespace
{
	constexpr std::size_t SerializedSeveritySize = sizeof(std::uint8_t);
	constexpr std::size_t SerializedStringLengthSize = sizeof(std::uint32_t);
	constexpr std::size_t DiagnosticHeaderSize =
		SerializedSeveritySize + SerializedStringLengthSize;

	[[nodiscard]] spk::Message::Type diagnosticMessageType() noexcept
	{
		return static_cast<spk::Message::Type>(
			Networking::MessageType::Diagnostic);
	}

	[[nodiscard]] bool validSeverity(std::uint8_t severity) noexcept
	{
		return severity <= static_cast<std::uint8_t>(Networking::Diagnostic::Severity::Error);
	}
}

Networking::Diagnostic::Builder::Builder(
	Severity severity,
	std::string message,
	spk::Message::RequestID requestID) :
	_severity(severity),
	_message(std::move(message)),
	_requestID(requestID)
{
}

Networking::Diagnostic Networking::Diagnostic::Builder::build() &&
{
	spk::Message::Writer writer(diagnosticMessageType());
	writer.setRequestID(_requestID);
	writer << static_cast<std::uint8_t>(_severity);
	writer << _message;
	return Diagnostic(std::move(writer).build());
}

Networking::Diagnostic::Diagnostic(
	spk::Message message,
	spk::Message::Type expectedType,
	bool allowTrailingPayload) :
	spk::Message(std::move(message))
{
	_validate(expectedType, allowTrailingPayload);
}

Networking::Diagnostic::Diagnostic(spk::Message message) :
	Diagnostic(
		std::move(message),
		diagnosticMessageType(),
		false)
{
}

void Networking::Diagnostic::_validate(
	spk::Message::Type expectedType,
	bool allowTrailingPayload) const
{
	if (type() != expectedType)
	{
		throw spk::Exception(
			"Networking::Diagnostic has an invalid Message type");
	}
	if (size() < DiagnosticHeaderSize)
	{
		throw spk::Exception(
			"Networking::Diagnostic payload is truncated");
	}

	const auto rawSeverity = reader().readAt<std::uint8_t>(0u);
	if (!validSeverity(rawSeverity))
	{
		throw spk::Exception(
			"Networking::Diagnostic contains an unknown severity");
	}

	const auto messageLength =
		reader().readAt<std::uint32_t>(SerializedSeveritySize);
	const std::size_t availableBytes =
		size() - DiagnosticHeaderSize;
	if (messageLength > availableBytes)
	{
		throw spk::Exception(
			"Networking::Diagnostic message is truncated");
	}

	const std::size_t expectedSize =
		DiagnosticHeaderSize +
		static_cast<std::size_t>(messageLength);
	if (!allowTrailingPayload && expectedSize != size())
	{
		throw spk::Exception(
			"Networking::Diagnostic contains unexpected trailing bytes");
	}
}

std::size_t Networking::Diagnostic::diagnosticSize() const
{
	const auto messageLength =
		reader().readAt<std::uint32_t>(SerializedSeveritySize);
	return DiagnosticHeaderSize + static_cast<std::size_t>(messageLength);
}

Networking::Diagnostic::Severity Networking::Diagnostic::severity() const
{
	return static_cast<Severity>(
		reader().readAt<std::uint8_t>(0u));
}

std::string Networking::Diagnostic::message() const
{
	const auto messageLength =
		reader().readAt<std::uint32_t>(SerializedSeveritySize);
	std::string result(
		static_cast<std::size_t>(messageLength),
		'\0');
	reader().readAt(
		DiagnosticHeaderSize,
		result.data(),
		result.size());
	return result;
}

spk::Message::Writer &Networking::operator<<(
	spk::Message::Writer &writer,
	const Diagnostic &diagnostic)
{
	writer << static_cast<std::uint8_t>(
		diagnostic.severity());
	writer << diagnostic.message();
	return writer;
}
