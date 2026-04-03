using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class AbilityPresentationUtility
{
	private const string PhysicalColor = "#F08A5D";
	private const string MagicalColor = "#9B5DE5";
	private const string HealingColor = "#27AE60";
	private const string ResourceColor = "#F2C94C";
	private const string StatusColor = "#56CCF2";
	private const string NeutralColor = "#EAEAEA";

	public static string GetDisplayName(Ability p_ability)
	{
		if (p_ability == null)
		{
			return "-----";
		}

		return string.IsNullOrWhiteSpace(p_ability.name)
			? "Unnamed Ability"
			: p_ability.name;
	}

	public static string FormatCost(Ability p_ability)
	{
		if (p_ability?.Cost == null)
		{
			return "Free";
		}

		List<string> parts = new List<string>(2);
		if (p_ability.Cost.Ability > 0)
		{
			parts.Add($"{p_ability.Cost.Ability} AP");
		}

		if (p_ability.Cost.Movement > 0)
		{
			parts.Add($"{p_ability.Cost.Movement} MP");
		}

		return parts.Count == 0
			? "Free"
			: string.Join(" / ", parts);
	}

	public static string FormatRange(Ability p_ability)
	{
		if (p_ability?.Range == null)
		{
			return "-----";
		}

		return $"{NicifyEnumName(p_ability.Range.Type)} {Mathf.Max(0, p_ability.Range.Value)}";
	}

	public static string FormatAreaOfEffect(Ability p_ability)
	{
		if (p_ability?.AreaOfEffect == null)
		{
			return "-----";
		}

		return $"{NicifyEnumName(p_ability.AreaOfEffect.Type)} {Mathf.Max(0, p_ability.AreaOfEffect.Value)}";
	}

	public static string FormatLineOfSight(Ability p_ability)
	{
		if (p_ability?.Range == null)
		{
			return "-----";
		}

		return p_ability.Range.RequireLineOfSight
			? "Requires LOS"
			: "Ignores LOS";
	}

	public static string BuildDescription(Ability p_ability)
	{
		if (p_ability == null)
		{
			return string.Empty;
		}

		StringBuilder builder = new StringBuilder();
		builder.Append("Target: ");
		builder.Append(FormatTargetProfile(p_ability.TargetProfile));

		if (p_ability.Effects == null || p_ability.Effects.Count == 0)
		{
			builder.Append("\nNo effects configured.");
			return builder.ToString();
		}

		for (int index = 0; index < p_ability.Effects.Count; index++)
		{
			string description = DescribeEffect(p_ability.Effects[index]);
			if (string.IsNullOrWhiteSpace(description))
			{
				continue;
			}

			builder.Append("\n- ");
			builder.Append(description);
		}

		return builder.ToString();
	}

	public static string DescribeEffect(Effect p_effect)
	{
		return p_effect switch
		{
			ApplyStatusEffect applyStatusEffect => DescribeApplyStatusEffect(applyStatusEffect),
			RemoveStatusEffect removeStatusEffect => DescribeRemoveStatusEffect(removeStatusEffect),
			ReviveEffect reviveEffect => $"Revive the target and restore {HighlightValue(reviveEffect.HealthRestored, HealingColor, "HP")}.",
			CleanseEffect cleanseEffect => $"Cleanse statuses tagged {HighlightNeutralList(cleanseEffect.TagsToCleanse)}.",
			ResourceChangeEffect resourceChangeEffect => DescribeResourceChangeEffect(resourceChangeEffect),
			MoveStatus moveStatus => moveStatus.ForceOrientation == MoveStatus.Orientation.TowardCaster
				? $"Pull the target by {HighlightNeutral(moveStatus.Force, "cell")}."
				: $"Push the target by {HighlightNeutral(moveStatus.Force, "cell")}.",
			SwapPositionEffect => "Swap position with the target.",
			TeleportEffect teleportEffect => DescribeTeleportEffect(teleportEffect),
			StealResourceEffect stealResourceEffect => $"Steal {HighlightValue(stealResourceEffect.Value, GetResourceColor(stealResourceEffect.ResourceTargeted), NicifyEnumName(stealResourceEffect.ResourceTargeted))} from the target.",
			ConsumeStatus consumeStatus => $"Consume {HighlightNeutral(consumeStatus.NbOfStackConsumed, "stack")} of {HighlightNamedAsset(consumeStatus.Status, StatusColor)}.",
			ChangeFormEffect changeFormEffect => $"Transform the target into <b>{FormatText(changeFormEffect.FormID, "another form")}</b>.",
			AdjustTurnBarTimeEffect adjustTurnBarTimeEffect => DescribeTurnBarTimeEffect(adjustTurnBarTimeEffect),
			AdjustTurnBarDurationEffect adjustTurnBarDurationEffect => DescribeTurnBarDurationEffect(adjustTurnBarDurationEffect),
			DamageTargetEffect damageTargetEffect => DescribeDamageEffect(damageTargetEffect),
			HealTargetEffect healTargetEffect => DescribeHealingEffect(healTargetEffect),
			PlaceInteractiveObjectEffect placeInteractiveObjectEffect => DescribePlaceInteractiveObjectEffect(placeInteractiveObjectEffect),
			RemoveInteractiveObjectEffect removeInteractiveObjectEffect => $"Remove interactive objects tagged {HighlightNeutralList(removeInteractiveObjectEffect.Tags)}.",
			null => string.Empty,
			_ => $"Trigger <b>{p_effect.GetType().Name}</b>."
		};
	}

	private static string DescribeApplyStatusEffect(ApplyStatusEffect p_effect)
	{
		string duration = FormatDuration(p_effect.Duration);
		return $"Apply {HighlightNamedAsset(p_effect.Status, StatusColor)} x{Mathf.Max(1, p_effect.StackCount)} for {duration}.";
	}

	private static string DescribeRemoveStatusEffect(RemoveStatusEffect p_effect)
	{
		return $"Remove up to {HighlightNeutral(Mathf.Max(1, p_effect.StackCount), "stack")} of {HighlightNamedAsset(p_effect.Status, StatusColor)}.";
	}

	private static string DescribeResourceChangeEffect(ResourceChangeEffect p_effect)
	{
		string resourceName = NicifyEnumName(p_effect.ResourceTargeted);
		string value = HighlightValue(Mathf.Abs(p_effect.Value), GetResourceColor(p_effect.ResourceTargeted), resourceName);

		if (p_effect.ResourceTargeted == ResourceChangeEffect.Target.Range)
		{
			return p_effect.Value >= 0
				? $"Increase the target bonus range by {value}."
				: $"Reduce the target bonus range by {value}.";
		}

		return p_effect.Value >= 0
			? $"Restore {value} to the target."
			: $"Remove {value} from the target.";
	}

	private static string DescribeTeleportEffect(TeleportEffect p_effect)
	{
		string coordinate = $"({p_effect.Destination.x}, {p_effect.Destination.y}, {p_effect.Destination.z})";
		return p_effect.RelativeToCaster
			? $"Teleport the target to <b>{coordinate}</b> relative to the caster."
			: $"Teleport the target to <b>{coordinate}</b>.";
	}

	private static string DescribeTurnBarTimeEffect(AdjustTurnBarTimeEffect p_effect)
	{
		string value = HighlightNeutral(Mathf.Abs(p_effect.Delta));
		return p_effect.Delta >= 0f
			? $"Advance the target turn bar by {value}."
			: $"Delay the target turn bar by {value}.";
	}

	private static string DescribeTurnBarDurationEffect(AdjustTurnBarDurationEffect p_effect)
	{
		string value = HighlightNeutral(Mathf.Abs(p_effect.Delta));
		return p_effect.Delta >= 0f
			? $"Increase the target turn bar duration by {value}."
			: $"Reduce the target turn bar duration by {value}.";
	}

	private static string DescribeDamageEffect(DamageTargetEffect p_effect)
	{
		string damageColor = p_effect.Input.DamageKind == MathFormula.DamageInput.Kind.Physical
			? PhysicalColor
			: MagicalColor;

		StringBuilder builder = new StringBuilder();
		builder.Append("Deal ");
		builder.Append(HighlightValue(p_effect.Input.BaseDamage, damageColor, $"{NicifyEnumName(p_effect.Input.DamageKind)} damage"));
		string scaling = FormatScaling(p_effect.Input.AttackRatio, p_effect.Input.MagicRatio);
		if (!string.IsNullOrEmpty(scaling))
		{
			builder.Append(" + ");
			builder.Append(scaling);
		}

		builder.Append(" to the target.");
		return builder.ToString();
	}

	private static string DescribeHealingEffect(HealTargetEffect p_effect)
	{
		StringBuilder builder = new StringBuilder();
		builder.Append("Heal the target for ");
		builder.Append(HighlightValue(p_effect.Input.BaseHealing, HealingColor, "HP"));
		string scaling = FormatScaling(p_effect.Input.AttackRatio, p_effect.Input.MagicRatio);
		if (!string.IsNullOrEmpty(scaling))
		{
			builder.Append(" + ");
			builder.Append(scaling);
		}

		builder.Append(".");
		return builder.ToString();
	}

	private static string DescribePlaceInteractiveObjectEffect(PlaceInteractiveObjectEffect p_effect)
	{
		string duration = FormatDuration(p_effect.Duration);
		return $"Place {HighlightNamedAsset(p_effect.InteractionObject, NeutralColor)} for {duration}.";
	}

	private static string FormatScaling(float p_attackRatio, float p_magicRatio)
	{
		List<string> parts = new List<string>(2);
		if (!Mathf.Approximately(p_attackRatio, 0f))
		{
			parts.Add($"{FormatPercent(p_attackRatio)} <b>ATK</b>");
		}

		if (!Mathf.Approximately(p_magicRatio, 0f))
		{
			parts.Add($"{FormatPercent(p_magicRatio)} <b>MAG</b>");
		}

		return string.Join(" + ", parts);
	}

	private static string FormatPercent(float p_ratio)
	{
		return $"{(p_ratio * 100f).ToString("0.##", CultureInfo.InvariantCulture)}%";
	}

	public static string FormatDuration(Duration p_duration)
	{
		if (p_duration == null)
		{
			return "an infinite duration";
		}

		return p_duration.Type switch
		{
			Duration.Kind.TurnBased => $"{Mathf.Max(1, p_duration.Turns)} turn{(Mathf.Max(1, p_duration.Turns) == 1 ? string.Empty : "s")}",
			Duration.Kind.Seconds => $"{Mathf.Max(0f, p_duration.Seconds).ToString("0.##", CultureInfo.InvariantCulture)}s",
			_ => "an infinite duration"
		};
	}

	private static string FormatTargetProfile(TargetProfile p_targetProfile)
	{
		return p_targetProfile switch
		{
			TargetProfile.Ally => "Ally",
			TargetProfile.Enemy => "Enemy",
			_ => "Anything"
		};
	}

	public static string NicifyEnumName(Enum p_value)
	{
		if (p_value == null)
		{
			return "Unknown";
		}

		string raw = p_value.ToString();
		StringBuilder builder = new StringBuilder(raw.Length + 4);
		for (int index = 0; index < raw.Length; index++)
		{
			char character = raw[index];
			if (index > 0 && char.IsUpper(character) && !char.IsUpper(raw[index - 1]))
			{
				builder.Append(' ');
			}

			builder.Append(character);
		}

		return builder.ToString();
	}

	private static string HighlightValue(int p_value, string p_color, string p_suffix)
	{
		return $"<b><color={p_color}>{p_value} {p_suffix}</color></b>";
	}

	private static string HighlightNeutral(int p_value, string p_suffix)
	{
		return HighlightNeutral($"{p_value} {p_suffix}{(p_value == 1 ? string.Empty : "s")}");
	}

	private static string HighlightNeutral(float p_value)
	{
		return HighlightNeutral(p_value.ToString("0.##", CultureInfo.InvariantCulture));
	}

	private static string HighlightNeutral(string p_value)
	{
		return $"<b><color={NeutralColor}>{p_value}</color></b>";
	}

	private static string HighlightNamedAsset(UnityEngine.Object p_asset, string p_color)
	{
		string assetName = p_asset != null && !string.IsNullOrWhiteSpace(p_asset.name)
			? p_asset.name
			: "unknown";
		return $"<b><color={p_color}>{assetName}</color></b>";
	}

	private static string HighlightNeutralList(IReadOnlyList<string> p_values)
	{
		if (p_values == null || p_values.Count == 0)
		{
			return HighlightNeutral("none");
		}

		return HighlightNeutral(string.Join(", ", p_values));
	}

	private static string GetResourceColor(ResourceChangeEffect.Target p_target)
	{
		return p_target switch
		{
			ResourceChangeEffect.Target.Range => StatusColor,
			_ => ResourceColor
		};
	}

	private static string GetResourceColor(StealResourceEffect.Target p_target)
	{
		return p_target switch
		{
			StealResourceEffect.Target.Health => HealingColor,
			StealResourceEffect.Target.Range => StatusColor,
			StealResourceEffect.Target.Stamina => NeutralColor,
			_ => ResourceColor
		};
	}

	private static string FormatText(string p_value, string p_fallback)
	{
		return string.IsNullOrWhiteSpace(p_value)
			? p_fallback
			: p_value;
	}
}
