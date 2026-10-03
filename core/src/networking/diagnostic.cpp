#include "erelia/core/networking/diagnostic.hpp"
#include <exception.hpp>
spk::Message::Writer &Networking::operator<<(spk::Message::Writer &writer, const Diagnostic &diagnostic)
{
	if (diagnostic.severity > Diagnostic::Severity::Error)
	{
		throw spk::Exception("Unknown Diagnostic severity");
	}
	return writer << diagnostic.severity << diagnostic.message;
}
const spk::Message::Reader &Networking::operator>>(const spk::Message::Reader &reader, Diagnostic &diagnostic)
{
	Diagnostic decoded;
	reader >> decoded.severity >> decoded.message;
	if (decoded.severity > Diagnostic::Severity::Error)
	{
		throw spk::Exception("Unknown Diagnostic severity");
	}
	diagnostic = std::move(decoded);
	return reader;
}
