using System;
using UnityEngine;

[Serializable]
public class PlayerData : ActorData
{
	[SerializeField] private Vector3Int worldCell = Vector3Int.zero;

	[SerializeReference]
	private CreatureUnit[] team = new CreatureUnit[GameRule.TeamMemberCount];

	[SerializeField]
	private CreatureStorage creatureStorage = new CreatureStorage();

	public Vector3Int WorldCell
	{
		get => worldCell;
		set => worldCell = value;
	}

	public CreatureUnit[] Team => team;

	public CreatureStorage CreatureStorage
	{
		get
		{
			creatureStorage ??= new CreatureStorage();
			return creatureStorage;
		}
	}

	public Vector3 WorldPosition => worldCell;

	public void CopyFrom(PlayerData p_other)
	{
		if (p_other == null)
		{
			worldCell = Vector3Int.zero;
			team = new CreatureUnit[GameRule.TeamMemberCount];
			creatureStorage = new CreatureStorage();
			return;
		}

		worldCell = p_other.WorldCell;
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
		var clonedTeam = new CreatureUnit[GameRule.TeamMemberCount];
		if (p_sourceTeam == null)
		{
			return clonedTeam;
		}

		Array.Copy(p_sourceTeam, clonedTeam, Mathf.Min(p_sourceTeam.Length, clonedTeam.Length));
		return clonedTeam;
	}
}