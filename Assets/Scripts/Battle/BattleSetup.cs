using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BattleSetup
{
	public IReadOnlyList<CreatureUnit> PlayerTeam { get; }
	public IReadOnlyList<EncounterUnit> EnemyTeam { get; }
	public IReadOnlyList<EncounterUnit> Team => EnemyTeam;
	public BoardData Board { get; }
	public Vector3 PlayerWorldPosition { get; }

	public BattleSetup(
		EncounterUnit[] p_team,
		BoardData p_board,
		Vector3 p_playerWorldPosition)
		: this(Array.Empty<CreatureUnit>(), p_team, p_board, p_playerWorldPosition)
	{
	}

	public BattleSetup(
		IReadOnlyList<CreatureUnit> p_playerTeam,
		IReadOnlyList<EncounterUnit> p_enemyTeam,
		BoardData p_board,
		Vector3 p_playerWorldPosition)
	{
		if (p_playerTeam == null)
		{
			throw new ArgumentNullException(nameof(p_playerTeam));
		}

		if (p_enemyTeam == null)
		{
			throw new ArgumentNullException(nameof(p_enemyTeam));
		}

		if (p_board == null)
		{
			throw new ArgumentNullException(nameof(p_board));
		}

		PlayerTeam = p_playerTeam;
		EnemyTeam = p_enemyTeam;
		Board = p_board;
		PlayerWorldPosition = p_playerWorldPosition;
	}

	public BattleSetup WithPlayerTeam(IReadOnlyList<CreatureUnit> p_playerTeam)
	{
		return new BattleSetup(p_playerTeam ?? Array.Empty<CreatureUnit>(), EnemyTeam, Board, PlayerWorldPosition);
	}
}
