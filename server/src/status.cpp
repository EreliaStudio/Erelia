#include "erelia/server/status.hpp"

#include "erelia/core/status.hpp"

int status() noexcept
{
	return erelia::core::status();
}
