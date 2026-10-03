#pragma once
#include <cstdint>
#include <network/message.hpp>
#include <string>
namespace Networking
{
	struct Diagnostic
	{
		enum class Severity : std::uint8_t
		{
			Trace = 0,
			Info = 1,
			Warning = 2,
			Error = 3
		};
		Severity severity = Severity::Info;
		std::string message;
	};
	spk::Message::Writer &operator<<(spk::Message::Writer &writer, const Diagnostic &diagnostic);
	const spk::Message::Reader &operator>>(const spk::Message::Reader &reader, Diagnostic &diagnostic);
}
