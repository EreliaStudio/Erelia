#include "erelia/core/service.hpp"

#include <design_pattern/singleton.hpp>
#include <threading/worker_pool.hpp>

spk::WorkerPool *Service::workerPool()
{
	if (spk::Singleton<spk::WorkerPool>::isInstanciated() == false)
	{
		spk::Singleton<spk::WorkerPool>::instanciate(new spk::WorkerPool());
	}

	return &spk::Singleton<spk::WorkerPool>::instance();
}
