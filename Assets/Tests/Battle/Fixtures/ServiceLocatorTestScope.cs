using System;

namespace Tests
{
	internal sealed class ServiceLocatorTestScope : IDisposable
	{
		public ServiceLocatorTestScope()
		{
			ServiceLocator.Create(new GameContext());
		}

		public void Dispose()
		{
			ServiceLocator.Destroy();
		}
	}
}
