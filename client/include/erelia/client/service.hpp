#pragma once

namespace spk
{
	class Client;
	class Translator;
}

namespace Service
{
	[[nodiscard]] spk::Client &client();
	[[nodiscard]] spk::Translator &translator();
}
