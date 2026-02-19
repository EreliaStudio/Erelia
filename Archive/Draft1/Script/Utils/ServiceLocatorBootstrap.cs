using UnityEngine;

namespace Utils
{
	public static class ServiceLocatorBootstrap
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void EnsureServiceLocator()
		{
			if (ServiceLocator.Instance != null)
			{
				return;
			}

			ServiceLocatorConfig config = ServiceLocatorConfig.LoadFromResources();
			ServiceLocator.Initialize(config);
		}
	}
}
