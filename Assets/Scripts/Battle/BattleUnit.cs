using System;
using System.Collections.Generic;

[Serializable]
public class BattleUnit : BattleObject
{
	public CreatureUnit SourceUnit;
	public int CurrentHealth = 0;
	public int CurrentActionPoints = 0;
	public int CurrentMovementPoints = 0;
	public List<BattleStatus> Statuses = new List<BattleStatus>();
	public bool IsDefeated = false;
};
