using System;
using UnityEngine;

namespace Erelia.Battle.Voxel
{
	[Serializable]
	public class CardinalPointSet
	{
		[SerializeField] private Vector3 positiveX = new Vector3(0.5f, 1f, 0.5f);
		[SerializeField] private Vector3 negativeX = new Vector3(0.5f, 1f, 0.5f);
		[SerializeField] private Vector3 positiveZ = new Vector3(0.5f, 1f, 0.5f);
		[SerializeField] private Vector3 negativeZ = new Vector3(0.5f, 1f, 0.5f);
		[SerializeField] private Vector3 stationary = new Vector3(0.5f, 1f, 0.5f);

		public Vector3 PositiveX => positiveX;
		public Vector3 NegativeX => negativeX;
		public Vector3 PositiveZ => positiveZ;
		public Vector3 NegativeZ => negativeZ;
		public Vector3 Stationary => stationary;

		public CardinalPointSet()
		{
		}

		public CardinalPointSet(
			Vector3 positiveX,
			Vector3 negativeX,
			Vector3 positiveZ,
			Vector3 negativeZ,
			Vector3 stationary)
		{
			this.positiveX = positiveX;
			this.negativeX = negativeX;
			this.positiveZ = positiveZ;
			this.negativeZ = negativeZ;
			this.stationary = stationary;
		}

		public Vector3 Get(Erelia.Battle.Voxel.CardinalPoint entryPoint)
		{
			switch (entryPoint)
			{
				case Erelia.Battle.Voxel.CardinalPoint.PositiveX:
					return positiveX;
				case Erelia.Battle.Voxel.CardinalPoint.NegativeX:
					return negativeX;
				case Erelia.Battle.Voxel.CardinalPoint.PositiveZ:
					return positiveZ;
				case Erelia.Battle.Voxel.CardinalPoint.NegativeZ:
					return negativeZ;
				case Erelia.Battle.Voxel.CardinalPoint.Stationary:
					return stationary;
				default:
					return stationary;
			}
		}

		public static CardinalPointSet CreateDefault()
		{
			Vector3 origin = new Vector3(0.5f, 1f, 0.5f);
			return new CardinalPointSet(origin, origin, origin, origin, origin);
		}
	}
}
