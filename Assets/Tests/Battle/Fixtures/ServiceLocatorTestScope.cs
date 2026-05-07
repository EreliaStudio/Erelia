using System;
using System.IO;

namespace Tests
{
	internal sealed class ServiceLocatorTestScope : IDisposable
	{
		private readonly string tempDirectory;

		public ServiceLocatorTestScope()
		{
			tempDirectory = Path.Combine(
				Path.GetTempPath(),
				"EreliaServiceLocatorTests",
				Guid.NewGuid().ToString("N"));
			ServiceLocator.CreateWithSaveFileOverride(new GameSaveData(), tempDirectory, "test-save.json");
		}

		public void Dispose()
		{
			ServiceLocator.Destroy();
			if (Directory.Exists(tempDirectory))
			{
				Directory.Delete(tempDirectory, true);
			}
		}
	}
}
