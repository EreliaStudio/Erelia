using UnityEngine;

namespace Erelia.Voxel
{
	public static class VoxelBootstrap
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			const string resourcesPath = "Voxel/VoxelLibrary";

			VoxelLibrary library = Resources.Load<VoxelLibrary>(resourcesPath);
			if (library == null)
			{
				Erelia.Logger.RaiseWarning($"VoxelLibrary not found at Resources path '{resourcesPath}'. Registry not initialized.");
				return;
			}

			int registered = library.RegisterAll(clearRegistry: true);
			Erelia.Logger.Log($"Voxel Registry initialized with {registered} definitions from '{library.name}'.");
		}
	}
}

