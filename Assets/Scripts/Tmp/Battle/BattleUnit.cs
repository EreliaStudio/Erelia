using System;
using System.Collections.Generic;

[Serializable]
public class BattleUnit : BattleObject
{
	public BattleUnit(CreatureUnit p_sourceUnit, BattleSide p_side)
	{
		SourceUnit = p_sourceUnit ?? throw new ArgumentNullException(nameof(p_sourceUnit));
		Side = p_side;

		BattleAttributes = new BattleAttributes(SourceUnit.Attributes);

		foreach (Status status in SourceUnit.PermanentPassives)
		{
			if (status == null)
			{
				continue;
			}

			Statuses.Add(status, 1, new Duration { Type = Duration.Kind.Infinite }, true);
		}
	}

	public CreatureUnit SourceUnit { get; }
	public BattleAttributes BattleAttributes { get; }
	public BattleStatuses Statuses { get; } = new();

	public IReadOnlyList<Ability> Abilities => SourceUnit.Abilities;
	public bool IsDefeated => BattleAttributes.Health.Current <= 0;
	public bool IsTurnReady => BattleAttributes.TurnBar.Current >= BattleAttributes.TurnBar.Max;
}
