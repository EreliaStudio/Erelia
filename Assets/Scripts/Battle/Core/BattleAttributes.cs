using System;
using System.Collections.Generic;

[Serializable]
public sealed class BattleAttributes : ObservableValue<BattleAttributes>
{
	public ObservableResource Health { get; } = new();
	public ObservableResource ActionPoints { get; } = new();
	public ObservableResource MovementPoints { get; } = new();
	public ObservableFloatResource TurnBar { get; } = new();
	public ObservableValue<int> BonusRange { get; } = new();
	public ObservableValue<int> Attack { get; } = new();
	public ObservableValue<int> Armor { get; } = new();
	public ObservableValue<int> ArmorPenetration { get; } = new();
	public ObservableValue<int> Magic { get; } = new();
	public ObservableValue<int> Resistance { get; } = new();
	public ObservableValue<int> ResistancePenetration { get; } = new();
	public ObservableValue<float> LifeSteal { get; } = new();
	public ObservableValue<float> Omnivamprism { get; } = new();
	public ObservableList<BattleShield> ActiveShields { get; } = new();

	public BattleAttributes(Attributes p_attributes)
	{
		Set(this, true);

		Health.Changed += _ => Notify();
		ActionPoints.Changed += _ => Notify();
		MovementPoints.Changed += _ => Notify();
		TurnBar.Changed += _ => Notify();
		BonusRange.Changed += _ => Notify();
		Attack.Changed += _ => Notify();
		Armor.Changed += _ => Notify();
		ArmorPenetration.Changed += _ => Notify();
		Magic.Changed += _ => Notify();
		Resistance.Changed += _ => Notify();
		ResistancePenetration.Changed += _ => Notify();
		LifeSteal.Changed += _ => Notify();
		Omnivamprism.Changed += _ => Notify();
		ActiveShields.Changed += _ => Notify();

		Setup(p_attributes);
	}

	public void Setup(Attributes p_attributes)
	{
		int maxHealth = p_attributes?.Health ?? 0;
		int maxActionPoints = p_attributes?.ActionPoints ?? 0;
		int maxMovementPoints = p_attributes?.Movement ?? 0;
		float turnBarDuration = MathFormula.ComputeBaseTurnBarDuration(p_attributes);

		Health.Set(maxHealth, maxHealth, true);
		ActionPoints.Set(maxActionPoints, maxActionPoints, true);
		MovementPoints.Set(maxMovementPoints, maxMovementPoints, true);
		TurnBar.Set(0f, turnBarDuration, true);

		BonusRange.Set(p_attributes?.BonusRange ?? 0, true);
		Attack.Set(p_attributes?.Attack ?? 0, true);
		Armor.Set(p_attributes?.Armor ?? 0, true);
		ArmorPenetration.Set(p_attributes?.ArmorPenetration ?? 0, true);
		Magic.Set(p_attributes?.Magic ?? 0, true);
		Resistance.Set(p_attributes?.Resistance ?? 0, true);
		ResistancePenetration.Set(p_attributes?.ResistancePenetration ?? 0, true);
		LifeSteal.Set(p_attributes?.LifeSteal ?? 0f, true);
		Omnivamprism.Set(p_attributes?.Omnivamprism ?? 0f, true);
	}

	public void AddShield(ShieldKind kind, int amount, int durationTurns)
	{
		if (amount <= 0)
		{
			return;
		}

		ActiveShields.Add(new BattleShield
		{
			Kind = kind,
			CurrentAmount = amount,
			RemainingTurns = durationTurns
		});
	}

	public BattleShieldAbsorptionResult AbsorbDamage(MathFormula.DamageInput.Kind damageKind, int incoming)
	{
		if (incoming <= 0 || ActiveShields.Count == 0)
		{
			return BattleShieldAbsorptionResult.Empty;
		}

		ShieldKind matchingKind = damageKind == MathFormula.DamageInput.Kind.Physical
			? ShieldKind.Physical
			: ShieldKind.Magical;

		int remaining = incoming;
		int totalAbsorbed = 0;
		List<ShieldKind> brokenShieldKinds = null;

		for (int index = ActiveShields.Count - 1; index >= 0 && remaining > 0; index--)
		{
			BattleShield shield = ActiveShields[index]?.Value;
			if (shield == null || shield.Kind != matchingKind)
			{
				continue;
			}

			int absorbed = Math.Min(shield.CurrentAmount, remaining);
			shield.CurrentAmount -= absorbed;
			remaining -= absorbed;
			totalAbsorbed += absorbed;

			if (shield.CurrentAmount <= 0)
			{
				brokenShieldKinds ??= new List<ShieldKind>();
				brokenShieldKinds.Add(matchingKind);
				ActiveShields.RemoveAt(index);
				continue;
			}

			ActiveShields.NotifyItemChangedAt(index);
		}

		if (totalAbsorbed <= 0)
		{
			return BattleShieldAbsorptionResult.Empty;
		}

		Notify();
		return new BattleShieldAbsorptionResult(totalAbsorbed, brokenShieldKinds);
	}

	public void AdvanceShieldDurations()
	{
		bool changed = false;
		for (int index = ActiveShields.Count - 1; index >= 0; index--)
		{
			BattleShield shield = ActiveShields[index]?.Value;
			if (shield == null || shield.IsInfinite)
			{
				continue;
			}

			shield.RemainingTurns--;
			changed = true;

			if (shield.RemainingTurns <= 0)
			{
				ActiveShields.RemoveAt(index);
				continue;
			}

			ActiveShields.NotifyItemChangedAt(index);
		}

		if (changed)
		{
			Notify();
		}
	}

	public void ClearShields()
	{
		ActiveShields.Clear();
	}
}
