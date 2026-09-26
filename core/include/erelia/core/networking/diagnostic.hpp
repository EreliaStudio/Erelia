#pragma once

#include <cstddef>
#include <cstdint>
#include <string>

#include <network/message.hpp>

namespace Networking
{
	class Diagnostic : public spk::Message
	{
	public:
		enum class Severity : std::uint8_t
		{
			Trace = 0,
			Info = 1,
			Warning = 2,
			Error = 3
		};

		class Builder final
		{
		private:
			Severity _severity;
			std::string _message;
			spk::Message::RequestID _requestID;

		public:
			Builder(
				Severity severity,
				std::string message,
				spk::Message::RequestID requestID = 0u);

			[[nodiscard]] Diagnostic build() &&;
		};

	protected:
		explicit Diagnostic(
			spk::Message::Type type,
			spk::Message::RequestID requestID);
		Diagnostic(
			spk::Message message,
			spk::Message::Type expectedType,
			bool allowTrailingPayload);

		[[nodiscard]] std::size_t diagnosticSize() const;

	private:
		void _validate(
			spk::Message::Type expectedType,
			bool allowTrailingPayload) const;

	public:
		explicit Diagnostic(spk::Message message);

		[[nodiscard]] Severity severity() const;
		[[nodiscard]] std::string message() const;
	};

	spk::Message &operator<<(
		spk::Message &message,
		const Diagnostic &diagnostic);
}
