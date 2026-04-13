using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class DebugBattleActionBarResourceOverride
{
	[SerializeField] private bool enabled;
	[SerializeField] private int current = 10;
	[SerializeField] private int max = 10;

	public void ApplyTo(ObservableResource p_resource)
	{
		if (!enabled || p_resource == null)
		{
			return;
		}

		p_resource.Set(current, max, true);
	}
}

[Serializable]
public sealed class DebugBattleActionBarStatusOverride
{
	public Status Status;
	public bool IsSourcePassive;
	public bool UseInfiniteStack;
	[Min(1)] public int Stack = 1;
	public Duration.Kind DurationKind = Duration.Kind.Infinite;
	[Min(1)] public int Turns = 1;
	[Min(0f)] public float Seconds = 1f;

	public int GetStackCount()
	{
		return UseInfiniteStack ? int.MaxValue : Mathf.Max(1, Stack);
	}

	public Duration CreateDuration()
	{
		return DurationKind switch
		{
			Duration.Kind.TurnBased => new Duration
			{
				Type = Duration.Kind.TurnBased,
				Turns = Mathf.Max(1, Turns)
			},
			Duration.Kind.Seconds => new Duration
			{
				Type = Duration.Kind.Seconds,
				Seconds = Mathf.Max(0f, Seconds)
			},
			_ => new Duration
			{
				Type = Duration.Kind.Infinite
			}
		};
	}
}

public sealed class DebugBattleActionBarHolder : MonoBehaviour
{
	[SerializeField] private BattleActionBarElementUI battleActionBarElementUI;
	[SerializeField] private CreatureUnit[] team = new CreatureUnit[6];
	[SerializeField] private int activeCreatureIndex = 0;
	[SerializeField] private BattleSide side = BattleSide.Player;
	[SerializeField] private bool includeSourcePassives = true;
	[SerializeField] private DebugBattleActionBarResourceOverride healthOverride = new DebugBattleActionBarResourceOverride();
	[SerializeField] private DebugBattleActionBarResourceOverride actionPointsOverride = new DebugBattleActionBarResourceOverride();
	[SerializeField] private DebugBattleActionBarResourceOverride movementPointsOverride = new DebugBattleActionBarResourceOverride();
	[SerializeField] private List<DebugBattleActionBarStatusOverride> extraStatuses = new List<DebugBattleActionBarStatusOverride>();

	private void Awake()
	{
		AutoResolveReferences();
	}

	private void OnValidate()
	{
		AutoResolveReferences();
	}

	private void Start()
	{
		Apply();
	}

	[ContextMenu("Auto Resolve")]
	public void AutoResolveReferences()
	{
		battleActionBarElementUI ??= GetComponentInChildren<BattleActionBarElementUI>(true);
	}

	[ContextMenu("Apply")]
	public void Apply()
	{
		if (battleActionBarElementUI == null)
		{
			return;
		}

		battleActionBarElementUI.Bind(CreatePreviewBattleUnit());
	}

	private BattleUnit CreatePreviewBattleUnit()
	{
		if (team == null || activeCreatureIndex < 0 || activeCreatureIndex >= team.Length)
		{
			return null;
		}

		CreatureUnit creatureUnit = team[activeCreatureIndex];
		if (creatureUnit == null)
		{
			return null;
		}

		FeatProgressionService.ApplyProgress(creatureUnit);
		BattleUnit battleUnit = new BattleUnit(creatureUnit, side);

		if (!includeSourcePassives)
		{
			RemoveSourcePassives(battleUnit, creatureUnit.PermanentPassives);
		}

		ApplyResourceOverrides(battleUnit);
		ApplyExtraStatuses(battleUnit);
		return battleUnit;
	}

	private void ApplyResourceOverrides(BattleUnit p_battleUnit)
	{
		if (p_battleUnit?.BattleAttributes == null)
		{
			return;
		}

		healthOverride?.ApplyTo(p_battleUnit.BattleAttributes.Health);
		actionPointsOverride?.ApplyTo(p_battleUnit.BattleAttributes.ActionPoints);
		movementPointsOverride?.ApplyTo(p_battleUnit.BattleAttributes.MovementPoints);
	}

	private void ApplyExtraStatuses(BattleUnit p_battleUnit)
	{
		if (p_battleUnit == null || extraStatuses == null)
		{
			return;
		}

		for (int index = 0; index < extraStatuses.Count; index++)
		{
			DebugBattleActionBarStatusOverride overrideStatus = extraStatuses[index];
			if (overrideStatus?.Status == null)
			{
				continue;
			}

			p_battleUnit.Statuses.Add(
				overrideStatus.Status,
				overrideStatus.GetStackCount(),
				overrideStatus.CreateDuration(),
				overrideStatus.IsSourcePassive);
		}
	}

	private static void RemoveSourcePassives(BattleUnit p_battleUnit, IReadOnlyList<Status> p_statuses)
	{
		if (p_battleUnit == null || p_statuses == null)
		{
			return;
		}

		for (int index = 0; index < p_statuses.Count; index++)
		{
			Status status = p_statuses[index];
			if (status == null)
			{
				continue;
			}

			p_battleUnit.Statuses.Remove(status, -1, true);
		}
	}
}
