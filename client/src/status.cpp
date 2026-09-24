#include "erelia/client/status.hpp"

#include "erelia/core/status.hpp"

int erelia::client::status() noexcept
{
	return erelia::core::status();
}
