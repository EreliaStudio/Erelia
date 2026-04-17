using System;
using UnityEngine;

public sealed class BattleSetup
{
	public EncounterUnit[] Team { get; }
	public BoardData Board { get; }
	public Vector3 PlayerWorldPosition { get; }

	public BattleSetup(
		EncounterUnit[] p_team,
		BoardData p_board,
		Vector3 p_playerWorldPosition)
	{
		if (p_team == null)
		{
			throw new ArgumentNullException(nameof(p_team));
		}

		if (p_board == null)
		{
			throw new ArgumentNullException(nameof(p_board));
		}

		Team = p_team;
		Board = p_board;
		PlayerWorldPosition = p_playerWorldPosition;
	}
}