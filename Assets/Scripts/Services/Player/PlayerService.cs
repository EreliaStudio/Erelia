using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class PlayerService
{
	private readonly GameContext gameContext;
	private Func<Vector3Int?> worldCellProvider;

	public PlayerService(GameContext p_gameContext)
	{
		gameContext = p_gameContext;
	}

	public PlayerData PlayerData => gameContext?.Player;
	public Vector3Int PlayerWorldCell => TryGetRuntimeWorldCell(out Vector3Int worldCell)
		? worldCell
		: PlayerData != null ? Vector3Int.FloorToInt(PlayerData.Position.Value) : Vector3Int.zero;

	public void Initialize()
	{
		EventCenter.TamingResolved += OnTamingResolved;
	}

	public void Shutdown()
	{
		EventCenter.TamingResolved -= OnTamingResolved;
		worldCellProvider = null;
	}

	public void BindWorldCellProvider(Func<Vector3Int?> p_worldCellProvider)
	{
		worldCellProvider = p_worldCellProvider;
	}

	public IReadOnlyList<CreatureUnit> GetActiveTeam()
	{
		return PlayerData?.Team ?? System.Array.Empty<CreatureUnit>();
	}

	public JObject ToJson(ReferenceRegistry p_registry)
	{
		return PlayerData?.ToJson(p_registry) ?? new JObject();
	}

	public bool LoadFromJson(JObject p_json, ReferenceRegistry p_registry)
	{
		if (PlayerData == null || p_json == null)
		{
			return false;
		}

		PlayerData.LoadFromJson(p_json, p_registry);
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

	private bool TryGetRuntimeWorldCell(out Vector3Int p_worldCell)
	{
		p_worldCell = default;
		if (worldCellProvider == null)
		{
			return false;
		}

		Vector3Int? candidate = worldCellProvider.Invoke();
		if (!candidate.HasValue)
		{
			return false;
		}

		p_worldCell = candidate.Value;
		return true;
	}
}
