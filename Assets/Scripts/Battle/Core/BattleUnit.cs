using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleUnit : BattleObject
{
	private Vector3Int boardPosition;
	private bool hasBoardPosition;

	public BattleUnit(CreatureUnit p_sourceUnit, BattleSide p_side)
	{
		SourceUnit = p_sourceUnit ?? throw new ArgumentNullException(nameof(p_sourceUnit));
		Side = p_side;

		BattleAttributes = new BattleAttributes(SourceUnit.Attributes);

		foreach (Status status in SourceUnit.PermanentPassives)
		{
			if (status == null)
			{
				continue;
			}

			Statuses.Add(status, 1, new Duration { Type = Duration.Kind.Infinite }, true);
		}
	}

	private readonly List<FeatRequirement.EventBase> pendingFeatEvents = new();

	public CreatureUnit SourceUnit { get; }
	public BattleAttributes BattleAttributes { get; }
	public BattleStatuses Statuses { get; } = new();
	public IReadOnlyList<FeatRequirement.EventBase> PendingFeatEvents => pendingFeatEvents;

	public IReadOnlyList<Ability> Abilities => SourceUnit.GetAbilities();
	public bool HasBoardPosition => hasBoardPosition;
	public Vector3Int BoardPosition => boardPosition;
	public bool IsDefeated => BattleAttributes.Health.Current <= 0;
	public bool IsTurnReady => BattleAttributes.TurnBar.Current >= BattleAttributes.TurnBar.Max;

	public event Action<BattleUnit, Vector3Int?> PositionChanged;

	public void RecordFeatEvent(FeatRequirement.EventBase featEvent)
	{
		if (featEvent == null)
		{
			return;
		}

		pendingFeatEvents.Add(featEvent);
	}

	public void ClearFeatEvents()
	{
		pendingFeatEvents.Clear();
	}

	public void SetBoardPosition(Vector3Int p_position)
	{
		boardPosition = p_position;
		hasBoardPosition = true;
		PositionChanged?.Invoke(this, p_position);
	}

	public void ClearBoardPosition()
	{
		if (!hasBoardPosition)
		{
			return;
		}

		hasBoardPosition = false;
		PositionChanged?.Invoke(this, null);
	}
}
