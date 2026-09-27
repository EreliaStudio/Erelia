#include "erelia/client/service.hpp"

#include <design_pattern/singleton.hpp>
#include <network/client.hpp>

spk::Client *Service::client()
{
	if (spk::Singleton<spk::Client>::isInstanciated() == false)
	{
		spk::Singleton<spk::Client>::instanciate(new spk::Client());
	}

	return &spk::Singleton<spk::Client>::instance();
}
