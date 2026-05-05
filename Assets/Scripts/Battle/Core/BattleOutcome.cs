using System.Collections.Generic;

public sealed class BattleOutcome
{
	public BattleSide Winner { get; }
	public IReadOnlyList<BattleUnit> SurvivingPlayerUnits { get; }
	public IReadOnlyList<BattleUnit> SurvivingEnemyUnits { get; }
	public IReadOnlyList<CreatureUnit> ImpressedCreatures { get; }
	public BattleStats Stats { get; }

	public BattleOutcome(
		BattleSide winner,
		IReadOnlyList<BattleUnit> survivingPlayerUnits,
		IReadOnlyList<BattleUnit> survivingEnemyUnits,
		BattleStats stats,
		IReadOnlyList<CreatureUnit> impressedCreatures = null)
	{
		Winner = winner;
		SurvivingPlayerUnits = survivingPlayerUnits ?? System.Array.Empty<BattleUnit>();
		SurvivingEnemyUnits = survivingEnemyUnits ?? System.Array.Empty<BattleUnit>();
		Stats = stats;
		ImpressedCreatures = impressedCreatures ?? System.Array.Empty<CreatureUnit>();
	}
}