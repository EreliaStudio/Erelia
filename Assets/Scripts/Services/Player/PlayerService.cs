#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class PlayerService
{
	private readonly GameContext gameContext;

	public PlayerService(GameContext p_gameContext)
	{
		gameContext = p_gameContext;
	}

	public PlayerData PlayerData => gameContext?.Player;
	public Vector3Int PlayerWorldCell => PlayerData?.WorldCell ?? Vector3Int.zero;

	public void Initialize()
	{
		EventCenter.TamingResolved += OnTamingResolved;
	}

	public void Shutdown()
	{
		EventCenter.TamingResolved -= OnTamingResolved;
	}

	public IReadOnlyList<CreatureUnit> GetActiveTeam()
	{
		return PlayerData?.Team ?? System.Array.Empty<CreatureUnit>();
	}

	public PlayerSaveData CreateSaveData()
	{
		return ToSaveData(PlayerData);
	}

	public bool LoadFromSaveData(PlayerSaveData p_saveData)
	{
		if (PlayerData == null || p_saveData == null)
		{
			return false;
		}

		PlayerData.CopyFrom(FromSaveData(p_saveData));
		return true;
	}

	public bool AddCreatureToTeamOrStorage(CreatureUnit p_creatureUnit)
	{
		if (PlayerData == null || p_creatureUnit == null)
		{
			return false;
		}

		bool hadOpenTeamSlot = HasOpenTeamSlot(PlayerData.Team);
		if (!PlayerData.AddCreatureToTeamOrStorage(p_creatureUnit))
		{
			return false;
		}

		EventCenter.EmitPlayerCreatureAdded(p_creatureUnit, hadOpenTeamSlot);
		return true;
	}

	private void OnTamingResolved(
		BattleContext p_battleContext,
		IReadOnlyList<CreatureUnit> p_recruits)
	{
		if (p_recruits == null)
		{
			return;
		}

		for (int index = 0; index < p_recruits.Count; index++)
		{
			AddCreatureToTeamOrStorage(p_recruits[index]);
		}
	}

	private static bool HasOpenTeamSlot(IReadOnlyList<CreatureUnit> p_team)
	{
		if (p_team == null)
		{
			return true;
		}

		for (int index = 0; index < p_team.Count; index++)
		{
			if (p_team[index] == null)
			{
				return true;
			}
		}

		return false;
	}

	private static PlayerSaveData ToSaveData(PlayerData p_player)
	{
		var saveData = new PlayerSaveData();
		if (p_player == null)
		{
			return saveData;
		}

		saveData.WorldCell = SerializableVector3Int.From(p_player.WorldCell);
		saveData.TeamSlots = BuildTeamSlots(p_player);
		saveData.StoredCreatures = BuildStoredCreatures(p_player);
		return saveData;
	}

	private static PlayerData FromSaveData(PlayerSaveData p_saveData)
	{
		var player = new PlayerData
		{
			WorldCell = p_saveData.WorldCell.ToVector3Int()
		};

		PopulateTeam(player, p_saveData.TeamSlots);
		PopulateStorage(player, p_saveData.StoredCreatures);
		return player;
	}

	private static List<CreatureSlotSaveData> BuildTeamSlots(PlayerData p_player)
	{
		var slots = new List<CreatureSlotSaveData>(GameRule.TeamMemberCount);
		CreatureUnit[] team = p_player?.Team;

		for (int index = 0; index < GameRule.TeamMemberCount; index++)
		{
			CreatureUnit unit = team != null && index < team.Length ? team[index] : null;
			slots.Add(new CreatureSlotSaveData
			{
				HasCreature = unit != null,
				Creature = ToCreatureSaveData(unit)
			});
		}

		return slots;
	}

	private static List<CreatureUnitSaveData> BuildStoredCreatures(PlayerData p_player)
	{
		var storedCreatures = new List<CreatureUnitSaveData>();
		if (p_player?.CreatureStorage == null)
		{
			return storedCreatures;
		}

		for (int index = 0; index < p_player.CreatureStorage.Count; index++)
		{
			CreatureUnitSaveData creatureSaveData = ToCreatureSaveData(p_player.CreatureStorage.GetAt(index));
			if (creatureSaveData != null)
			{
				storedCreatures.Add(creatureSaveData);
			}
		}

		return storedCreatures;
	}

	private static CreatureUnitSaveData ToCreatureSaveData(CreatureUnit p_unit)
	{
		if (p_unit?.Species == null)
		{
			return null;
		}

		return new CreatureUnitSaveData
		{
			SpeciesResourceId = GetResourceId(p_unit.Species),
			CurrentFormId = p_unit.CurrentFormID ?? string.Empty,
			FeatBoardProgress = ToProgressSaveData(p_unit.FeatBoardProgress)
		};
	}

	private static FeatBoardProgressSaveData ToProgressSaveData(FeatBoardProgress p_progress)
	{
		var saveData = new FeatBoardProgressSaveData();
		if (p_progress?.NodeProgress == null)
		{
			return saveData;
		}

		for (int nodeIndex = 0; nodeIndex < p_progress.NodeProgress.Count; nodeIndex++)
		{
			FeatNodeProgress nodeProgress = p_progress.NodeProgress[nodeIndex];
			if (nodeProgress == null)
			{
				continue;
			}

			var nodeSaveData = new FeatNodeProgressSaveData
			{
				NodeId = nodeProgress.NodeId ?? string.Empty,
				CompletionCount = nodeProgress.CompletionCount
			};

			if (nodeProgress.RequirementProgress != null)
			{
				for (int requirementIndex = 0; requirementIndex < nodeProgress.RequirementProgress.Count; requirementIndex++)
				{
					FeatRequirementProgress requirementProgress = nodeProgress.RequirementProgress[requirementIndex];
					if (requirementProgress == null)
					{
						continue;
					}

					nodeSaveData.RequirementProgress.Add(new FeatRequirementProgressSaveData
					{
						CurrentProgress = requirementProgress.CurrentProgress,
						CompletedRepeatCount = requirementProgress.CompletedRepeatCount
					});
				}
			}

			saveData.NodeProgress.Add(nodeSaveData);
		}

		return saveData;
	}

	private static void PopulateTeam(PlayerData p_player, List<CreatureSlotSaveData> p_slots)
	{
		if (p_player == null || p_slots == null)
		{
			return;
		}

		int slotCount = Math.Min(GameRule.TeamMemberCount, p_slots.Count);
		for (int index = 0; index < slotCount; index++)
		{
			CreatureSlotSaveData slot = p_slots[index];
			if (slot == null || !slot.HasCreature)
			{
				continue;
			}

			p_player.Team[index] = FromCreatureSaveData(slot.Creature);
		}
	}

	private static void PopulateStorage(PlayerData p_player, List<CreatureUnitSaveData> p_storedCreatures)
	{
		if (p_player == null || p_storedCreatures == null)
		{
			return;
		}

		for (int index = 0; index < p_storedCreatures.Count; index++)
		{
			CreatureUnit creature = FromCreatureSaveData(p_storedCreatures[index]);
			if (creature != null)
			{
				p_player.CreatureStorage.Add(creature);
			}
		}
	}

	private static CreatureUnit FromCreatureSaveData(CreatureUnitSaveData p_saveData)
	{
		if (p_saveData == null || string.IsNullOrWhiteSpace(p_saveData.SpeciesResourceId))
		{
			return null;
		}

		CreatureSpecies species = LoadResource<CreatureSpecies>(p_saveData.SpeciesResourceId);
		if (species == null)
		{
			Logger.LogError(
				$"[PlayerService] Could not load saved creature species [{p_saveData.SpeciesResourceId}].",
				Logger.Severity.Warning);
			return null;
		}

		var creature = new CreatureUnit
		{
			Species = species,
			CurrentFormID = p_saveData.CurrentFormId ?? string.Empty,
			FeatBoardProgress = FromProgressSaveData(p_saveData.FeatBoardProgress, species)
		};

		FeatBoardService.ApplyProgress(creature);
		return creature;
	}

	private static FeatBoardProgress FromProgressSaveData(FeatBoardProgressSaveData p_saveData, CreatureSpecies p_species)
	{
		var progress = new FeatBoardProgress();
		if (p_saveData?.NodeProgress == null || p_species?.FeatBoard == null)
		{
			return progress;
		}

		for (int nodeIndex = 0; nodeIndex < p_saveData.NodeProgress.Count; nodeIndex++)
		{
			FeatNodeProgressSaveData nodeSaveData = p_saveData.NodeProgress[nodeIndex];
			if (nodeSaveData == null || string.IsNullOrWhiteSpace(nodeSaveData.NodeId))
			{
				continue;
			}

			FeatNode node = p_species.FeatBoard.GetNode(nodeSaveData.NodeId);
			if (node == null)
			{
				continue;
			}

			var nodeProgress = new FeatNodeProgress(node)
			{
				CompletionCount = Math.Max(0, nodeSaveData.CompletionCount)
			};

			ApplyRequirementProgress(nodeProgress, nodeSaveData.RequirementProgress);
			progress.NodeProgress.Add(nodeProgress);
		}

		return progress;
	}

	private static void ApplyRequirementProgress(
		FeatNodeProgress p_nodeProgress,
		List<FeatRequirementProgressSaveData> p_requirementProgress)
	{
		if (p_nodeProgress?.RequirementProgress == null || p_requirementProgress == null)
		{
			return;
		}

		int requirementCount = Math.Min(p_nodeProgress.RequirementProgress.Count, p_requirementProgress.Count);
		for (int requirementIndex = 0; requirementIndex < requirementCount; requirementIndex++)
		{
			FeatRequirementProgress requirementProgress = p_nodeProgress.RequirementProgress[requirementIndex];
			FeatRequirementProgressSaveData savedProgress = p_requirementProgress[requirementIndex];
			if (requirementProgress == null || savedProgress == null)
			{
				continue;
			}

			requirementProgress.CurrentProgress = savedProgress.CurrentProgress;
			requirementProgress.CompletedRepeatCount = Math.Max(0, savedProgress.CompletedRepeatCount);
		}
	}

	private static string GetResourceId(Object p_asset)
	{
		if (p_asset == null)
		{
			return string.Empty;
		}

		return TryGetResourcePath(p_asset, out string resourcePath)
			? resourcePath
			: p_asset.name;
	}

	private static T LoadResource<T>(string p_resourceId)
		where T : Object
	{
		if (string.IsNullOrWhiteSpace(p_resourceId))
		{
			return null;
		}

		T resource = Resources.Load<T>(p_resourceId);
		if (resource != null)
		{
			return resource;
		}

		T[] resources = Resources.LoadAll<T>(string.Empty);
		for (int index = 0; index < resources.Length; index++)
		{
			T candidate = resources[index];
			if (candidate == null)
			{
				continue;
			}

			if (string.Equals(candidate.name, p_resourceId, StringComparison.Ordinal))
			{
				return candidate;
			}

			if (TryGetResourcePath(candidate, out string candidatePath) &&
				string.Equals(candidatePath, p_resourceId, StringComparison.Ordinal))
			{
				return candidate;
			}
		}

		return null;
	}

	private static bool TryGetResourcePath(Object p_asset, out string p_resourcePath)
	{
		p_resourcePath = string.Empty;

#if UNITY_EDITOR
		string assetPath = AssetDatabase.GetAssetPath(p_asset);
		if (string.IsNullOrWhiteSpace(assetPath))
		{
			return false;
		}

		string normalizedPath = assetPath.Replace('\\', '/');
		const string resourcesMarker = "/Resources/";
		int resourcesIndex = normalizedPath.IndexOf(resourcesMarker, StringComparison.OrdinalIgnoreCase);
		if (resourcesIndex < 0)
		{
			return false;
		}

		int resourcePathStart = resourcesIndex + resourcesMarker.Length;
		p_resourcePath = Path.ChangeExtension(normalizedPath.Substring(resourcePathStart), null)
			.Replace('\\', '/');
		return !string.IsNullOrWhiteSpace(p_resourcePath);
#else
		return false;
#endif
	}
}
