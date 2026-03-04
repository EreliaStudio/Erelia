using UnityEngine;

namespace Erelia.Exploration.World
{
	/// <summary>
	/// Lazy loader for the voxel registry used in exploration.
	/// Loads the registry from Resources on first access and rebuilds its runtime lookup.
	/// </summary>
	public static class VoxelRegistry
	{
		/// <summary>
		/// Resources path (without extension) to the voxel registry asset.
		/// </summary>
		public const string ResourcePath = "Voxel/VoxelRegistry";

		/// <summary>
		/// Cached registry instance.
		/// </summary>
		private static Erelia.Core.VoxelKit.Registry instance;

		/// <summary>
		/// Gets the loaded voxel registry instance.
		/// </summary>
		public static Erelia.Core.VoxelKit.Registry Instance
		{
			get
			{
				// Return cached instance if already loaded.
				if (instance != null)
				{
					return instance;
				}

				// Load from Resources.
				instance = Resources.Load<Erelia.Core.VoxelKit.Registry>(ResourcePath);
				if (instance == null)
				{
					Debug.LogWarning($"Voxel registry not found at Resources/{ResourcePath}.asset");
					return null;
				}

				// Ensure runtime dictionary is built.
				instance.RebuildFromEntries();
				return instance;
			}
		}
	}
}
