using System;

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
}
