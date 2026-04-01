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

	public int MaxHealth => SourceUnit?.Attributes?.Health ?? 0;
	public int MaxActionPoints => SourceUnit?.Attributes?.ActionPoints ?? 0;
	public int MaxMovementPoints => SourceUnit?.Attributes?.Movement ?? 0;

	public void InitializeFromSourceUnit()
	{
		if (SourceUnit == null)
		{
			CurrentHealth = 0;
			CurrentActionPoints = 0;
			CurrentMovementPoints = 0;
			Statuses = new List<BattleStatus>();
			IsDefeated = false;
			return;
		}

		SourceUnit.EnsureInitialized();

		CurrentHealth = MaxHealth;
		CurrentActionPoints = MaxActionPoints;
		CurrentMovementPoints = MaxMovementPoints;
		IsDefeated = CurrentHealth <= 0;

		Statuses = new List<BattleStatus>();

		if (SourceUnit.PermanentPassives == null)
		{
			return;
		}

		for (int index = 0; index < SourceUnit.PermanentPassives.Count; index++)
		{
			Status status = SourceUnit.PermanentPassives[index];
			if (status == null)
			{
				continue;
			}

			Statuses.Add(new BattleStatus
			{
				Status = status,
				Stack = 1
			});
		}
	}

	public static BattleUnit CreateFromSource(CreatureUnit p_sourceUnit, BattleSide p_side = BattleSide.Neutral)
	{
		BattleUnit battleUnit = new BattleUnit
		{
			SourceUnit = p_sourceUnit,
			Side = p_side
		};

		battleUnit.InitializeFromSourceUnit();
		return battleUnit;
	}
};
