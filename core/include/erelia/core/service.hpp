#pragma once

namespace spk
{
	class WorkerPool;
}

namespace Service
{
	[[nodiscard]] spk::WorkerPool *workerPool();
}
