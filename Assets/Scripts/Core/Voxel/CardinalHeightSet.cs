using System;
using UnityEngine;

[Serializable]
public class CardinalHeightSet
{
	[Serializable]
	public enum Direction
	{
		PositiveX,
		NegativeX,
		PositiveZ,
		NegativeZ,
		Stationary
	}

	[SerializeField] private float positiveX = 1f;
	[SerializeField] private float negativeX = 1f;
	[SerializeField] private float positiveZ = 1f;
	[SerializeField] private float negativeZ = 1f;
	[SerializeField] private float stationary = 1f;

	public float PositiveX => positiveX;
	public float NegativeX => negativeX;
	public float PositiveZ => positiveZ;
	public float NegativeZ => negativeZ;
	public float Stationary => stationary;

	public CardinalHeightSet()
	{
	}

	public CardinalHeightSet(
		float positiveX,
		float negativeX,
		float positiveZ,
		float negativeZ,
		float stationary)
	{
		this.positiveX = positiveX;
		this.negativeX = negativeX;
		this.positiveZ = positiveZ;
		this.negativeZ = negativeZ;
		this.stationary = stationary;
	}

	public float Get(CardinalHeightSet.Direction p_direction)
	{
		switch (p_direction)
		{
			case CardinalHeightSet.Direction.PositiveX:
				return positiveX;
			case CardinalHeightSet.Direction.NegativeX:
				return negativeX;
			case CardinalHeightSet.Direction.PositiveZ:
				return positiveZ;
			case CardinalHeightSet.Direction.NegativeZ:
				return negativeZ;
			case CardinalHeightSet.Direction.Stationary:
			default:
				return stationary;
		}
	}

	public static CardinalHeightSet CreateDefault()
	{
		return new CardinalHeightSet(1f, 1f, 1f, 1f, 1f);
	}
}
