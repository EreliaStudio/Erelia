using System.Collections.Generic;

public sealed class PlayerService
{
	private readonly GameContext gameContext;

	public PlayerService(GameContext p_gameContext)
	{
		gameContext = p_gameContext;
	}

	public PlayerData PlayerData => gameContext?.Player;

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
}
