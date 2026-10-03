#include "erelia/client/service.hpp"

#include "erelia/client/requesting_world_provider.hpp"

#include <network/client.hpp>
#include <system/translator.hpp>

spk::Client &Service::client()
{
	static spk::Client client;
	return client;
}

spk::Translator &Service::translator()
{
	static spk::Translator translator;
	return translator;
}

WorldService &Service::clientWorldService()
{
	static WorldService worlds(
		RequestingWorldProvider{});
	return worlds;
}
