using System;
using UnityEngine;

[Serializable]
public sealed class GameContext
{
	private readonly WorldContext world = new WorldContext();
	private readonly PlayerData player = new PlayerData();

	public WorldContext World => world;
	public PlayerData Player => player;

	public static GameContext CreateFromSave(GameSaveData saveData)
	{
		var context = new GameContext();
		context.LoadFromSave(saveData);
		return context;
	}

	public void LoadFromSave(GameSaveData saveData)
	{
		saveData ??= new GameSaveData();
		world.ApplySeed(saveData.WorldSeed);
		player.WorldCell = saveData.PlayerWorldCell;
	}

	public bool EnsurePlayerSpawn(GameSaveData saveData, WorldData worldData, VoxelRegistry voxelRegistry)
	{
		if (saveData.PlayerSpawnResolved)
		{
			return true;
		}

		if (!TryFindSurfaceCell(worldData, voxelRegistry, player.WorldCell.x, player.WorldCell.z, out Vector3Int spawnCell))
		{
			return false;
		}

		saveData.SetResolvedSpawn(spawnCell);
		player.WorldCell = spawnCell;
		return true;
	}

	private static bool TryFindSurfaceCell(WorldData worldData, VoxelRegistry voxelRegistry, int x, int z, out Vector3Int spawnCell)
	{
		spawnCell = Vector3Int.zero;

		if (worldData == null || voxelRegistry == null)
		{
			return false;
		}

		for (int y = ChunkData.FixedSizeY - 1; y >= 0; y--)
		{
			Vector3Int candidate = new(x, y, z);
			if (!worldData.TryGetCell(candidate, out VoxelCell cell))
			{
				continue;
			}

			if (!VoxelTraversalUtility.IsSolid(cell, voxelRegistry))
			{
				continue;
			}

			spawnCell = new Vector3Int(x, y + 1, z);
			return true;
		}

		return false;
	}
}
