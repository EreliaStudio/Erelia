using System;
using System.Collections.Generic;

public static partial class EventCenter
{
	public static event Action<BattleContext, WildBattleUnit> CreatureImpressed;
	public static event Action<BattleContext, IReadOnlyList<CreatureUnit>> TamingResolved;

	public static void EmitCreatureImpressed(BattleContext p_battleContext, WildBattleUnit p_wildUnit)
	{
		CreatureImpressed?.Invoke(p_battleContext, p_wildUnit);
	}

	public static void EmitTamingResolved(
		BattleContext p_battleContext,
		IReadOnlyList<CreatureUnit> p_recruits)
	{
		TamingResolved?.Invoke(p_battleContext, p_recruits);
	}
}
