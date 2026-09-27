#include "erelia/client/service.hpp"

#include <design_pattern/singleton.hpp>
#include <network/client.hpp>
#include <system/translator.hpp>

spk::Client *Service::client()
{
	if (spk::Singleton<spk::Client>::isInstanciated() == false)
	{
		spk::Singleton<spk::Client>::instanciate(new spk::Client());
	}

	return &spk::Singleton<spk::Client>::instance();
}

spk::Translator *Service::translator()
{
	if (spk::Singleton<spk::Translator>::isInstanciated() == false)
	{
		spk::Singleton<spk::Translator>::instanciate(new spk::Translator());
	}

	return &spk::Singleton<spk::Translator>::instance();
}
