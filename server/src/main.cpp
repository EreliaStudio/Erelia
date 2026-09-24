#include "erelia/server/status.hpp"

#include <design_pattern/singleton.hpp>
#include <threading/worker_pool.hpp>

int main()
{
	spk::Singleton<spk::WorkerPool>::instanciate(
		new spk::WorkerPool());
	return status();
}
