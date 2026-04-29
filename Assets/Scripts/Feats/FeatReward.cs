using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class FeatReward
{
	public abstract void Apply(CreatureUnit p_creatureUnit);
}

[Serializable]
public class BonusStatsReward : FeatReward
{
	public enum AttributeType
	{
		Health,
		ActionPoints,
		Movement,
		Attack,
		Armor,
		ArmorPenetration,
		Magic,
		Resistance,
		ResistancePenetration,
		BonusRange,
		Recovery,
		BonusHealing,
		LifeSteal,
		Omnivamprism,
		TimeEffectResistance
	};

	public AttributeType Attribute;
	public float Value = 1f;

	public override void Apply(CreatureUnit p_creatureUnit)
	{
		if (p_creatureUnit == null || p_creatureUnit.Attributes == null)
		{
			return;
		}

		int intValue = Mathf.RoundToInt(Value);

		switch (Attribute)
		{
			case AttributeType.Health:
				p_creatureUnit.Attributes.Health += intValue;
				break;

			case AttributeType.ActionPoints:
				p_creatureUnit.Attributes.ActionPoints += intValue;
				break;

			case AttributeType.Movement:
				p_creatureUnit.Attributes.Movement += intValue;
				break;

			case AttributeType.Attack:
				p_creatureUnit.Attributes.Attack += intValue;
				break;

			case AttributeType.Armor:
				p_creatureUnit.Attributes.Armor += intValue;
				break;

			case AttributeType.ArmorPenetration:
				p_creatureUnit.Attributes.ArmorPenetration += intValue;
				break;

			case AttributeType.Magic:
				p_creatureUnit.Attributes.Magic += intValue;
				break;

			case AttributeType.Resistance:
				p_creatureUnit.Attributes.Resistance += intValue;
				break;

			case AttributeType.ResistancePenetration:
				p_creatureUnit.Attributes.ResistancePenetration += intValue;
				break;

			case AttributeType.BonusRange:
				p_creatureUnit.Attributes.BonusRange += intValue;
				break;

			case AttributeType.Recovery:
				p_creatureUnit.Attributes.Recovery += Value;
				break;

			case AttributeType.BonusHealing:
				p_creatureUnit.Attributes.BonusHealing += Value;
				break;

			case AttributeType.LifeSteal:
				p_creatureUnit.Attributes.LifeSteal += Value;
				break;

			case AttributeType.Omnivamprism:
				p_creatureUnit.Attributes.Omnivamprism += Value;
				break;

			case AttributeType.TimeEffectResistance:
				p_creatureUnit.Attributes.TimeEffectResistance += Value;
				break;
		}
	}
}

[Serializable]
public class AbilityReward : FeatReward
{
	public Ability Ability;

	public override void Apply(CreatureUnit p_creatureUnit)
	{
		if (p_creatureUnit == null || Ability == null)
		{
			return;
		}

		p_creatureUnit.AddAbility(Ability);
	}
}

[Serializable]
public class RemoveAbilityReward : FeatReward
{
	public Ability Ability;

	public override void Apply(CreatureUnit p_creatureUnit)
	{
		if (p_creatureUnit == null || Ability == null)
		{
			return;
		}

		p_creatureUnit.RemoveAbility(Ability);
	}
}

[Serializable]
public class PassiveReward : FeatReward
{
	public Status Status;

	public override void Apply(CreatureUnit p_creatureUnit)
	{
		if (p_creatureUnit == null || Status == null)
		{
			return;
		}

		if (p_creatureUnit.PermanentPassives == null)
		{
			p_creatureUnit.PermanentPassives = new List<Status>();
		}

		if (p_creatureUnit.PermanentPassives.Contains(Status) == false)
		{
			p_creatureUnit.PermanentPassives.Add(Status);
		}
	}
}

[Serializable]
public class ChangeFormReward : FeatReward
{
	public string FormKey;

	public override void Apply(CreatureUnit p_creatureUnit)
	{
		if (p_creatureUnit == null || p_creatureUnit.Species == null)
		{
			return;
		}

		if (string.IsNullOrEmpty(FormKey))
		{
			return;
		}

		if (p_creatureUnit.Species.Forms == null)
		{
			return;
		}

		if (p_creatureUnit.Species.Forms.ContainsKey(FormKey) == false)
		{
			return;
		}

		p_creatureUnit.CurrentFormID = FormKey;
	}
}
