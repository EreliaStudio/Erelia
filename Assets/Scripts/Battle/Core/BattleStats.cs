using System.Collections.Generic;

public sealed class BattleStats
{
	private readonly Dictionary<BattleUnit, UnitStats> statsByUnit = new Dictionary<BattleUnit, UnitStats>();

	public sealed class UnitStats
	{
		public int MoveCount { get; internal set; }
		public int AbilityCastCount { get; internal set; }
		public int DamageDealt { get; internal set; }
		public int HealingDone { get; internal set; }
	}

	public UnitStats GetStats(BattleUnit unit)
	{
		if (unit == null)
		{
			return null;
		}

		if (!statsByUnit.TryGetValue(unit, out UnitStats stats))
		{
			stats = new UnitStats();
			statsByUnit[unit] = stats;
		}

		return stats;
	}

	public void RecordMove(BattleUnit unit)
	{
		if (unit == null)
		{
			return;
		}

		GetStats(unit).MoveCount++;
	}

	public void RecordAbilityCast(BattleUnit unit)
	{
		if (unit == null)
		{
			return;
		}

		GetStats(unit).AbilityCastCount++;
	}

	public void RecordDamageDealt(BattleUnit unit, int amount)
	{
		if (unit == null || amount <= 0)
		{
			return;
		}

		GetStats(unit).DamageDealt += amount;
	}

	public void RecordHealingDone(BattleUnit unit, int amount)
	{
		if (unit == null || amount <= 0)
		{
			return;
		}

		GetStats(unit).HealingDone += amount;
	}

	public void Reset()
	{
		statsByUnit.Clear();
	}
}
