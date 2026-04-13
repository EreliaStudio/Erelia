using UnityEngine;

namespace Erelia.Exploration.World
{
	public static class VoxelCatalog
	{
		public const string ResourcePath = "Voxel/VoxelRegistry";

		private static Erelia.Core.Voxel.VoxelRegistry instance;

		public static Erelia.Core.Voxel.VoxelRegistry Instance
		{
			get
			{
				if (instance != null)
				{
					return instance;
				}

				instance = Resources.Load<Erelia.Core.Voxel.VoxelRegistry>(ResourcePath);
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


