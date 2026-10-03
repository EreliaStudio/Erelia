#include "erelia/client/service.hpp"

Client::EventCenter &Service::clientEventCenter()
{
	static Client::EventCenter events;
	return events;
}
