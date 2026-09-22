#include "erelia/server/status.hpp"

#include "erelia/core/status.hpp"

int erelia::server::status() noexcept
{
	return erelia::core::status();
}
