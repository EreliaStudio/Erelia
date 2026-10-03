#pragma once

#include "erelia/core/event_center.hpp"

namespace spk
{
	class WorkerPool;
}

namespace Service
{
	[[nodiscard]] spk::WorkerPool &workerPool();
	[[nodiscard]] Core::EventCenter &coreEventCenter();
}
