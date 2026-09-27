#include "erelia/core/service.hpp"

#include "erelia/core/translation_engine.hpp"

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

TranslationEngine *Service::translationEngine()
{
	if (spk::Singleton<TranslationEngine>::isInstanciated() == false)
	{
		spk::Singleton<TranslationEngine>::instanciate(new TranslationEngine());
	}

	return &spk::Singleton<TranslationEngine>::instance();
}
