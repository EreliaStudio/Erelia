using System;

public static partial class EventCenter
{
	public static event Action<BattleContext, BattleSide> EncounterResolved;

	public static void EmitEncounterResolved(BattleContext p_battleContext, BattleSide p_winner)
	{
		EncounterResolved?.Invoke(p_battleContext, p_winner);
	}
}
