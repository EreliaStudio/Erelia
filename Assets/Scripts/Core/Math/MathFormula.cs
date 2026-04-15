using System;
using UnityEngine;

public static class MathFormula
{
	[Serializable]
	public struct DamageInput
	{
		public enum Kind
		{
			Physical,
			Magical
		}

		public int BaseDamage;
		public Kind DamageKind;
		public float AttackRatio;
		public float MagicRatio;
	}

	[Serializable]
	public struct HealingInput
	{
		public int BaseHealing;
		public float AttackRatio;
		public float MagicRatio;
	}

	public const float MitigationScaling = 10f;
	public const float TimeEffectResistanceScaling = 10f;
	public const float MinimumTurnBarDuration = 0.1f;

	public static int ComputeDamage(Attributes p_casterAttributes, Attributes p_targetAttributes, DamageInput p_input)
	{
		float rawDamage = Mathf.Max(
			0f,
			p_input.BaseDamage +
			GetAttackContribution(p_casterAttributes, p_input.AttackRatio) +
			GetMagicContribution(p_casterAttributes, p_input.MagicRatio));

		float reductionRatio = ComputeDamageReductionRatio(p_casterAttributes, p_targetAttributes, p_input.DamageKind);
		return Mathf.Max(0, Mathf.RoundToInt(rawDamage * (1f - reductionRatio)));
	}

	public static int ComputeHealing(Attributes p_casterAttributes, HealingInput p_input)
	{
		if (p_input.BaseHealing <= 0)
		{
			return 0;
		}

		float rawHealing = Mathf.Max(
			0f,
			p_input.BaseHealing +
			GetAttackContribution(p_casterAttributes, p_input.AttackRatio) +
			GetMagicContribution(p_casterAttributes, p_input.MagicRatio));

		return ApplyBonusHealing(rawHealing, p_casterAttributes);
	}

	public static int ComputeVampirismHealing(Attributes p_casterAttributes, DamageInput.Kind p_damageKind, int p_appliedDamage)
	{
		if (p_appliedDamage <= 0 || p_casterAttributes == null)
		{
			return 0;
		}

		float ratio = p_damageKind == DamageInput.Kind.Physical
			? p_casterAttributes.LifeSteal
			: p_casterAttributes.Omnivamprism;

		if (ratio <= 0f)
		{
			return 0;
		}

		float rawHealing = Mathf.Max(0f, p_appliedDamage * ratio);
		return ApplyBonusHealing(rawHealing, p_casterAttributes);
	}

	public static float ComputeDamageReductionRatio(Attributes p_targetAttributes, DamageInput.Kind p_damageKind)
	{
		return ComputeDamageReductionRatio(null, p_targetAttributes, p_damageKind);
	}

	public static float ComputeDamageReductionRatio(Attributes p_casterAttributes, Attributes p_targetAttributes, DamageInput.Kind p_damageKind)
	{
		if (p_targetAttributes == null)
		{
			return 0f;
		}

		if (p_damageKind == DamageInput.Kind.Physical)
		{
			int effectiveArmor = Mathf.Max(0, p_targetAttributes.Armor - (p_casterAttributes?.ArmorPenetration ?? 0));
			return ComputePhysicalReductionRatio(effectiveArmor);
		}

		int effectiveResistance = Mathf.Max(0, p_targetAttributes.Resistance - (p_casterAttributes?.ResistancePenetration ?? 0));
		return ComputeMagicalReductionRatio(effectiveResistance);
	}

	public static float ComputePhysicalReductionRatio(int p_armor)
	{
		return ComputeReductionRatio(p_armor);
	}

	public static float ComputeMagicalReductionRatio(int p_resistance)
	{
		return ComputeReductionRatio(p_resistance);
	}

	public static float ComputeTurnBarTimeDelta(float p_delta, Attributes p_targetAttributes)
	{
		return ComputeTimeEffectDelta(p_delta, p_targetAttributes);
	}

	public static float ComputeTurnBarDurationDelta(float p_delta, Attributes p_targetAttributes)
	{
		return ComputeTimeEffectDelta(p_delta, p_targetAttributes);
	}

	public static float ComputeBaseTurnBarDuration(Attributes p_attributes)
	{
		return Mathf.Max(MinimumTurnBarDuration, p_attributes?.Recovery ?? MinimumTurnBarDuration);
	}

	public static float ComputeTimeEffectResistanceRatio(Attributes p_targetAttributes)
	{
		if (p_targetAttributes == null)
		{
			return 0f;
		}

		return ComputeReductionRatio(p_targetAttributes.TimeEffectResistance, TimeEffectResistanceScaling);
	}

	private static float GetAttackContribution(Attributes p_casterAttributes, float p_attackRatio)
	{
		return p_casterAttributes != null ? p_casterAttributes.Attack * p_attackRatio : 0f;
	}

	private static float GetMagicContribution(Attributes p_casterAttributes, float p_magicRatio)
	{
		return p_casterAttributes != null ? p_casterAttributes.Magic * p_magicRatio : 0f;
	}

	private static float ComputeReductionRatio(int p_defensiveStat)
	{
		return ComputeReductionRatio(p_defensiveStat, MitigationScaling);
	}

	private static float ComputeReductionRatio(float p_defensiveStat, float p_scaling)
	{
		if (p_defensiveStat <= 0)
		{
			return 0f;
		}

		return Mathf.Clamp01(p_defensiveStat / (p_defensiveStat + p_scaling));
	}

	private static float ComputeTimeEffectDelta(float p_delta, Attributes p_targetAttributes)
	{
		float reductionRatio = ComputeTimeEffectResistanceRatio(p_targetAttributes);
		return p_delta * (1f - reductionRatio);
	}

	private static int ApplyBonusHealing(float p_rawHealing, Attributes p_casterAttributes)
	{
		float bonusHealingMultiplier = 1f + Mathf.Max(-0.95f, p_casterAttributes?.BonusHealing ?? 0f);
		return Mathf.Max(0, Mathf.RoundToInt(p_rawHealing * bonusHealingMultiplier));
	}
}
