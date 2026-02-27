using UnityEngine;

namespace Erelia
{
	public static class VoxelRegistry
	{
		public const string ResourcePath = "Voxel/VoxelRegistry";

		private static VoxelKit.Registry instance;

		public static VoxelKit.Registry Instance
		{
			get
			{
				if (instance != null)
				{
					return instance;
				}

				instance = Resources.Load<VoxelKit.Registry>(ResourcePath);
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
