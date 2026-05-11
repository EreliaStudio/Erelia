using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class PlayerData : ActorData
{
	[UnityEngine.SerializeReference]
	private CreatureUnit[] team = new CreatureUnit[GameRule.TeamMemberCount];

	[UnityEngine.SerializeField]
	private CreatureStorage creatureStorage = new CreatureStorage();

	public CreatureUnit[] Team => team;

	public CreatureStorage CreatureStorage
	{
		get
		{
			creatureStorage ??= new CreatureStorage();
			return creatureStorage;
		}
	}

	protected override void OnCellReached(Vector3Int p_cellPosition)
	{
		EventCenter.EmitPlayerMoved(p_cellPosition);
	}

	public void CopyFrom(PlayerData p_other)
	{
		if (p_other == null)
		{
			SetPosition(default, true);
			team = new CreatureUnit[GameRule.TeamMemberCount];
			creatureStorage = new CreatureStorage();
			return;
		}

		SetPosition(p_other.Position.Value, true);
		team = CloneTeam(p_other.Team);
		creatureStorage = p_other.CreatureStorage.Clone();
	}

	public bool AddCreatureToTeamOrStorage(CreatureUnit p_creatureUnit)
	{
		if (p_creatureUnit == null)
		{
			return false;
		}

		EnsureTeamInitialized();

		for (int index = 0; index < team.Length; index++)
		{
			if (team[index] != null)
			{
				continue;
			}

			team[index] = p_creatureUnit;
			return true;
		}

		CreatureStorage.Add(p_creatureUnit);
		return true;
	}

	public JObject ToJson(ReferenceRegistry p_registry)
	{
		EnsureTeamInitialized();

		JArray teamArray = new JArray();
		for (int index = 0; index < GameRule.TeamMemberCount; index++)
		{
			CreatureUnit unit = index < team.Length ? team[index] : null;
			JObject slot = new JObject { ["hasCreature"] = unit != null };
			if (unit != null)
			{
				slot["creature"] = unit.ToJson();
			}
			teamArray.Add(slot);
		}

		JArray storageArray = new JArray();
		if (creatureStorage != null)
		{
			for (int index = 0; index < creatureStorage.Count; index++)
			{
				CreatureUnit unit = creatureStorage.GetAt(index);
				if (unit != null)
				{
					storageArray.Add(unit.ToJson());
				}
			}
		}

		return new JObject
		{
			["position"] = SaveHelper.ToJson(Position.Value),
			["team"] = teamArray,
			["storage"] = storageArray
		};
	}

	public void LoadFromJson(JObject p_json, ReferenceRegistry p_registry)
	{
		if (p_json == null)
		{
			return;
		}

		SetPosition(SaveHelper.ToVector3(p_json["position"] as JObject), true);
		team = new CreatureUnit[GameRule.TeamMemberCount];
		creatureStorage = new CreatureStorage();

		JArray teamArray = p_json["team"] as JArray;
		if (teamArray != null)
		{
			int count = Math.Min(GameRule.TeamMemberCount, teamArray.Count);
			for (int index = 0; index < count; index++)
			{
				JObject slot = teamArray[index] as JObject;
				if (slot == null || !(slot["hasCreature"]?.Value<bool>() ?? false))
				{
					continue;
				}

				team[index] = CreatureUnit.FromJson(slot["creature"] as JObject, p_registry);
			}
		}

		JArray storageArray = p_json["storage"] as JArray;
		if (storageArray != null)
		{
			foreach (JObject creatureJson in storageArray)
			{
				CreatureUnit unit = CreatureUnit.FromJson(creatureJson, p_registry);
				if (unit != null)
				{
					creatureStorage.Add(unit);
				}
			}
		}
	}

	private void EnsureTeamInitialized()
	{
		if (team == null || team.Length != GameRule.TeamMemberCount)
		{
			CreatureUnit[] resizedTeam = new CreatureUnit[GameRule.TeamMemberCount];

			if (team != null)
			{
				Array.Copy(team, resizedTeam, Mathf.Min(team.Length, resizedTeam.Length));
			}

			team = resizedTeam;
		}
	}

	private static CreatureUnit[] CloneTeam(CreatureUnit[] p_sourceTeam)
	{
		CreatureUnit[] clonedTeam = new CreatureUnit[GameRule.TeamMemberCount];
		if (p_sourceTeam == null)
		{
			return clonedTeam;
		}

		Array.Copy(p_sourceTeam, clonedTeam, Mathf.Min(p_sourceTeam.Length, clonedTeam.Length));
		return clonedTeam;
	}
}
