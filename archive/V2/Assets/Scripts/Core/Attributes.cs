using System;

[Serializable]
public class Attributes
{
	public Attributes()
	{
	}

	public Attributes(Attributes p_source)
	{
		if (p_source == null)
		{
			return;
		}

		Health = p_source.Health;
		ActionPoints = p_source.ActionPoints;
		Movement = p_source.Movement;
		Attack = p_source.Attack;
		Armor = p_source.Armor;
		ArmorPenetration = p_source.ArmorPenetration;
		Magic = p_source.Magic;
		Resistance = p_source.Resistance;
		ResistancePenetration = p_source.ResistancePenetration;
		BonusRange = p_source.BonusRange;
		Recovery = p_source.Recovery;
		BonusHealing = p_source.BonusHealing;
		LifeSteal = p_source.LifeSteal;
		Omnivamprism = p_source.Omnivamprism;
		TimeEffectResistance = p_source.TimeEffectResistance;
	}

	public int Health = 10;

	public int ActionPoints = 6;

	public int Movement = 3;

	public int Attack = 0;
	public int Armor = 0;
	public int ArmorPenetration = 0;
	public int Magic = 0;
	public int Resistance = 0;
	public int ResistancePenetration = 0;
	public int BonusRange = 0;

	public float Recovery = 4f;
	public float BonusHealing = 0f;
	public float LifeSteal = 0f;
	public float Omnivamprism = 0f;
	public float TimeEffectResistance = 0f;
};
