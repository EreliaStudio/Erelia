using UnityEngine;

namespace Erelia.Exploration.World
{
	public static class VoxelRegistry
	{
		public const string ResourcePath = "Voxel/VoxelRegistry";

		private static Erelia.Core.VoxelKit.Registry instance;

		public static Erelia.Core.VoxelKit.Registry Instance
		{
			get
			{
				if (instance != null)
				{
					return instance;
				}

				instance = Resources.Load<Erelia.Core.VoxelKit.Registry>(ResourcePath);
				if (instance == null)
				{
					Debug.LogWarning($"Voxel registry not found at Resources/{ResourcePath}.asset");
					return null;
				}

				instance.RebuildFromEntries();
				return instance;
			}
		}
	}
}
