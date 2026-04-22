using System;
using UnityEngine;

[Serializable]
public readonly struct AbilityCastLegality
{
	public enum Failure
	{
		None,
		InvalidContext,
		NoActiveUnit,
		SourceNotPlaced,
		InsufficientResources,
		OutOfBoard,
		OutOfRange,
		BlockedByLineOfSight,
		InvalidTargetProfile
	}

	public AbilityCastLegality(bool isValid, Failure failure, Vector3Int targetCell)
	{
		IsValid = isValid;
		FailureReason = failure;
		TargetCell = targetCell;
	}

	public bool IsValid { get; }
	public Failure FailureReason { get; }
	public Vector3Int TargetCell { get; }

	public static AbilityCastLegality Valid(Vector3Int targetCell)
	{
		return new AbilityCastLegality(true, Failure.None, targetCell);
	}

	public static AbilityCastLegality Invalid(Failure failure, Vector3Int targetCell)
	{
		return new AbilityCastLegality(false, failure, targetCell);
	}
}
