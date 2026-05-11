using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class GameSaveFileData
{
	public const int CurrentVersion = 1;

	public int Version = CurrentVersion;
	public int WorldSeed;
	public SerializableVector3Int RespawnPoint;
	public PlayerSaveData Player = new PlayerSaveData();
}

[Serializable]
public sealed class PlayerSaveData
{
	public Vector3 Position;
	public List<CreatureSlotSaveData> TeamSlots = new List<CreatureSlotSaveData>();
	public List<CreatureUnitSaveData> StoredCreatures = new List<CreatureUnitSaveData>();
}

[Serializable]
public sealed class CreatureSlotSaveData
{
	public bool HasCreature;
	public CreatureUnitSaveData Creature;
}

[Serializable]
public sealed class CreatureUnitSaveData
{
	public string SpeciesResourceId = string.Empty;
	public string CurrentFormId = string.Empty;
	public FeatBoardProgressSaveData FeatBoardProgress = new FeatBoardProgressSaveData();
}

[Serializable]
public sealed class FeatBoardProgressSaveData
{
	public List<FeatNodeProgressSaveData> NodeProgress = new List<FeatNodeProgressSaveData>();
}

[Serializable]
public sealed class FeatNodeProgressSaveData
{
	public string NodeId = string.Empty;
	public int CompletionCount;
	public List<FeatRequirementProgressSaveData> RequirementProgress = new List<FeatRequirementProgressSaveData>();
}

[Serializable]
public sealed class FeatRequirementProgressSaveData
{
	public float CurrentProgress;
	public int CompletedRepeatCount;
}

[Serializable]
public struct SerializableVector3Int
{
	public int X;
	public int Y;
	public int Z;

	public static SerializableVector3Int From(Vector3Int p_vector)
	{
		return new SerializableVector3Int
		{
			X = p_vector.x,
			Y = p_vector.y,
			Z = p_vector.z
		};
	}

	public Vector3Int ToVector3Int()
	{
		return new Vector3Int(X, Y, Z);
	}
}
