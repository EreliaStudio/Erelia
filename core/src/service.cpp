#include "erelia/core/service.hpp"

#include <threading/worker_pool.hpp>

spk::WorkerPool &Service::workerPool()
{
	static spk::WorkerPool workerPool;
	return workerPool;
}
