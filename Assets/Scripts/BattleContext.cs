using System;
using System.Collections.Generic;

[Serializable]
public class BattleContext
{
	public List<BattleUnit> PlayerUnits = new List<BattleUnit>();
	public List<BattleUnit> EnemyUnits = new List<BattleUnit>();
}
