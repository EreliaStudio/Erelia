#pragma once

#include "erelia/core/event_center.hpp"
#include "erelia/core/world_collection.hpp"

namespace spk
{
	class WorkerPool;
}

namespace Service
{
	[[nodiscard]] spk::WorkerPool &workerPool();
	[[nodiscard]] Core::EventCenter &coreEventCenter();
	[[nodiscard]] WorldCollection &worldCollection();
}
