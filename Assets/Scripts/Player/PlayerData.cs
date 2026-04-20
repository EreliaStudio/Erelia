using System;
using UnityEngine;

[Serializable]
public class PlayerData : ActorData
{
	[SerializeField] private Vector3Int worldCell = Vector3Int.zero;
	[SerializeReference] private CreatureUnit[] team = new CreatureUnit[GameRule.TeamMemberCount];

	public Vector3Int WorldCell
	{
		get => worldCell;
		set => worldCell = value;
	}

	public CreatureUnit[] Team => team;
	public Vector3 WorldPosition => worldCell;

	public void CopyFrom(PlayerData p_other)
	{
		if (p_other == null)
		{
			worldCell = Vector3Int.zero;
			team = new CreatureUnit[GameRule.TeamMemberCount];
			return;
		}

		worldCell = p_other.WorldCell;
		team = CloneTeam(p_other.Team);
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
