#pragma once

namespace spk
{
	class Client;
	class WorkerPool;
}

namespace Service
{
	[[nodiscard]] spk::WorkerPool *workerPool();
	[[nodiscard]] spk::Client *client();
}
