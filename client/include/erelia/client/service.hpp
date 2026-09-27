#pragma once

namespace spk
{
	class Client;
}

namespace Service
{
	[[nodiscard]] spk::Client *client();
}
