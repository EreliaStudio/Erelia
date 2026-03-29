using System;

[Serializable]
public class BattleUnit
{
	public CreatureUnit SourceUnit;
	public int CurrentHealth = 0;
	public int CurrentActionPoints = 0;
	public int CurrentMovementPoints = 0;
	public bool IsDefeated = false;
};
