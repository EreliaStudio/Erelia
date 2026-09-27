#pragma once

class TranslationEngine;

namespace spk
{
	class WorkerPool;
}

namespace Service
{
	[[nodiscard]] spk::WorkerPool *workerPool();
	[[nodiscard]] TranslationEngine *translationEngine();
}
