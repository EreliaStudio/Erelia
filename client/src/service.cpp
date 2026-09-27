#include "erelia/client/service.hpp"

#include <design_pattern/singleton.hpp>
#include <network/client.hpp>
#include <threading/worker_pool.hpp>

spk::WorkerPool *Service::workerPool()
{
	if (spk::Singleton<spk::WorkerPool>::isInstanciated() == false)
	{
		spk::Singleton<spk::WorkerPool>::instanciate(new spk::WorkerPool());
	}

	return &spk::Singleton<spk::WorkerPool>::instance();
}

spk::Client *Service::client()
{
	if (spk::Singleton<spk::Client>::isInstanciated() == false)
	{
		spk::Singleton<spk::Client>::instanciate(new spk::Client());
	}

	return &spk::Singleton<spk::Client>::instance();
}
