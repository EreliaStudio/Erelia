using System;

public static partial class EventCenter
{
	public static event Action<CreatureUnit, bool> PlayerCreatureAdded;

	public static void EmitPlayerCreatureAdded(CreatureUnit p_creatureUnit, bool p_addedToActiveTeam)
	{
		PlayerCreatureAdded?.Invoke(p_creatureUnit, p_addedToActiveTeam);
	}
}
