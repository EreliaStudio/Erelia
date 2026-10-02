#include "erelia/core/service.hpp"

Core::EventCenter &Service::coreEventCenter()
{
	static Core::EventCenter events;
	return events;
}
