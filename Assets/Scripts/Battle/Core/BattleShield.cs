using System;
using System.Collections.Generic;

public enum ShieldKind { Physical, Magical }

[Serializable]
public class BattleShield
{
	public ShieldKind Kind;
	public int CurrentAmount;
	public int RemainingTurns; // -1 = infinite, 0+ = turns left

	public bool IsInfinite => RemainingTurns < 0;
}

public readonly struct BattleShieldAbsorptionResult
{
	public static BattleShieldAbsorptionResult Empty { get; } =
		new BattleShieldAbsorptionResult(0, Array.Empty<ShieldKind>());

	public BattleShieldAbsorptionResult(int amountAbsorbed, IReadOnlyList<ShieldKind> brokenShieldKinds)
	{
		AmountAbsorbed = Math.Max(0, amountAbsorbed);
		BrokenShieldKinds = brokenShieldKinds ?? Array.Empty<ShieldKind>();
	}

	public int AmountAbsorbed { get; }
	public IReadOnlyList<ShieldKind> BrokenShieldKinds { get; }
}
