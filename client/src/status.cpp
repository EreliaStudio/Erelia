#include "erelia/client/status.hpp"

#include "erelia/core/status.hpp"

int status() noexcept
{
	return erelia::core::status();
}
