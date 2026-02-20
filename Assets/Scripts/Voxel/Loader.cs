using System;
using UnityEngine;

namespace Erelia.Voxel
{
	public static class Loader
	{
		public static int LoadLibrary(string resourcesPath)
		{
			if (string.IsNullOrWhiteSpace(resourcesPath))
			{
				throw new ArgumentException("resourcesPath cannot be null/empty.", nameof(resourcesPath));
			}

			VoxelLibrary library = Resources.Load<VoxelLibrary>(resourcesPath);
			if (library == null)
			{
				Debug.LogWarning($"VoxelLibrary not found at Resources path '{resourcesPath}'.");
				return 0;
			}

			return library.RegisterAll(clearRegistry: true);
		}
	}
}

