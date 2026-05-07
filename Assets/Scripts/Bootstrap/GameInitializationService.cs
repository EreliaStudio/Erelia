using UnityEngine;

public static class GameInitializationService
{
	public static bool TryInitializeNewGameSave(GameSaveData p_saveData, WorldData p_worldData, VoxelRegistry p_voxelRegistry)
	{
		if (p_saveData == null || p_worldData == null || p_voxelRegistry == null)
		{
			return false;
		}

		Vector3Int anchorCell = p_saveData.PlayerWorldCell;
		if (!TryFindSurfaceCell(p_worldData, p_voxelRegistry, anchorCell.x, anchorCell.z, out Vector3Int spawnCell))
		{
			return false;
		}

		p_saveData.SetPlayerWorldCell(spawnCell);
		p_saveData.SetRespawnPoint(spawnCell);
		return true;
	}

	private static bool TryFindSurfaceCell(WorldData p_worldData, VoxelRegistry p_voxelRegistry, int p_x, int p_z, out Vector3Int p_spawnCell)
	{
		p_spawnCell = Vector3Int.zero;

		if (p_worldData == null || p_voxelRegistry == null)
		{
			return false;
		}

		for (int y = ChunkData.FixedSizeY - 1; y >= 0; y--)
		{
			Vector3Int candidate = new Vector3Int(p_x, y, p_z);
			if (!p_worldData.TryGetCell(candidate, out VoxelCell cell))
			{
				continue;
			}

			if (!VoxelTraversalUtility.IsSolid(cell, p_voxelRegistry))
			{
				continue;
			}

			p_spawnCell = new Vector3Int(p_x, y + 1, p_z);
			return true;
		}

		return false;
	}
}
