using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class StatusPresentationUtility
{
	public static string GetDisplayName(Status p_status)
	{
		if (p_status == null)
		{
			return "-----";
		}

		return string.IsNullOrWhiteSpace(p_status.name)
			? "Unnamed Status"
			: p_status.name;
	}

	public static string FormatStackLabel(BattleStatus p_status)
	{
		if (p_status == null || p_status.Stack <= 0 || p_status.Stack == int.MaxValue)
		{
			return string.Empty;
		}

		return p_status.Stack.ToString(CultureInfo.InvariantCulture);
	}

	public static string FormatDurationLabel(BattleStatus p_status)
	{
		if (p_status?.RemainingDuration == null)
		{
			return string.Empty;
		}

		return p_status.RemainingDuration.Type switch
		{
			Duration.Kind.TurnBased => $"{Mathf.Max(1, p_status.RemainingDuration.Turns)}T",
			Duration.Kind.Seconds => $"{Mathf.Max(0f, p_status.RemainingDuration.Seconds).ToString("0.##", CultureInfo.InvariantCulture)}s",
			_ => string.Empty
		};
	}

	public static string FormatStackSummary(BattleStatus p_status)
	{
		if (p_status == null)
		{
			return "Stacks: -----";
		}

		if (p_status.Stack == int.MaxValue)
		{
			return "Stacks: Infinite";
		}

		return $"Stacks: {Mathf.Max(0, p_status.Stack)}";
	}

	public static string FormatDurationSummary(BattleStatus p_status)
	{
		if (p_status?.RemainingDuration == null)
		{
			return "Duration: -----";
		}

		return p_status.RemainingDuration.Type switch
		{
			Duration.Kind.TurnBased => $"Duration: {Mathf.Max(1, p_status.RemainingDuration.Turns)}T",
			Duration.Kind.Seconds => $"Duration: {Mathf.Max(0f, p_status.RemainingDuration.Seconds).ToString("0.##", CultureInfo.InvariantCulture)}s",
			_ => "Duration: Infinite"
		};
	}

	public static string FormatTriggerSummary(Status p_status)
	{
		if (p_status == null)
		{
			return "Trigger: -----";
		}

		return $"Trigger: {AbilityPresentationUtility.NicifyEnumName(p_status.HookPoint)}";
	}

	public static string FormatTagSummary(Status p_status)
	{
		if (p_status?.Tags == null || p_status.Tags.Count == 0)
		{
			return "Tags: None";
		}

		return $"Tags: {string.Join(", ", p_status.Tags)}";
	}

	public static string BuildDescription(BattleStatus p_status)
	{
		return BuildDescription(p_status?.Status);
	}

	public static string BuildDescription(Status p_status)
	{
		if (p_status == null)
		{
			return string.Empty;
		}

		if (p_status.Effects == null || p_status.Effects.Count == 0)
		{
			return "No effects configured.";
		}

		StringBuilder builder = new StringBuilder();
		for (int index = 0; index < p_status.Effects.Count; index++)
		{
			string description = AbilityPresentationUtility.DescribeEffect(p_status.Effects[index]);
			if (string.IsNullOrWhiteSpace(description))
			{
				continue;
			}

			if (builder.Length > 0)
			{
				builder.Append('\n');
			}

			builder.Append("- ");
			builder.Append(description);
		}

		return builder.ToString();
	}
}
