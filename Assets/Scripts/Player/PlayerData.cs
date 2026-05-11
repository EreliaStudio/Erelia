using System;
using UnityEngine;

[Serializable]
public class PlayerData : ActorData
{
	[SerializeReference]
	private CreatureUnit[] team = new CreatureUnit[GameRule.TeamMemberCount];

	[SerializeField]
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
