using System;
using UnityEngine;

namespace Erelia.Battle.Voxel
{
	/// <summary>
	/// Holds offset vectors for each cardinal point.
	/// Used by mask shapes to compute placement or entry positions.
	/// </summary>
	[Serializable]
	public class CardinalPointSet
	{
		/// <summary>
		/// Offset for the positive X entry point.
		/// </summary>
		[SerializeField] private Vector3 positiveX = new Vector3(0.5f, 1f, 0.5f);
		/// <summary>
		/// Offset for the negative X entry point.
		/// </summary>
		[SerializeField] private Vector3 negativeX = new Vector3(0.5f, 1f, 0.5f);
		/// <summary>
		/// Offset for the positive Z entry point.
		/// </summary>
		[SerializeField] private Vector3 positiveZ = new Vector3(0.5f, 1f, 0.5f);
		/// <summary>
		/// Offset for the negative Z entry point.
		/// </summary>
		[SerializeField] private Vector3 negativeZ = new Vector3(0.5f, 1f, 0.5f);
		/// <summary>
		/// Offset for the stationary entry point.
		/// </summary>
		[SerializeField] private Vector3 stationary = new Vector3(0.5f, 1f, 0.5f);

		/// <summary>
		/// Gets the positive X offset.
		/// </summary>
		public Vector3 PositiveX => positiveX;
		/// <summary>
		/// Gets the negative X offset.
		/// </summary>
		public Vector3 NegativeX => negativeX;
		/// <summary>
		/// Gets the positive Z offset.
		/// </summary>
		public Vector3 PositiveZ => positiveZ;
		/// <summary>
		/// Gets the negative Z offset.
		/// </summary>
		public Vector3 NegativeZ => negativeZ;
		/// <summary>
		/// Gets the stationary offset.
		/// </summary>
		public Vector3 Stationary => stationary;

		/// <summary>
		/// Creates a default cardinal point set.
		/// </summary>
		public CardinalPointSet()
		{
			// Use default serialized values.
		}

		/// <summary>
		/// Creates a cardinal point set with explicit offsets.
		/// </summary>
		public CardinalPointSet(
			Vector3 positiveX,
			Vector3 negativeX,
			Vector3 positiveZ,
			Vector3 negativeZ,
			Vector3 stationary)
		{
			// Store explicit offsets.
			this.positiveX = positiveX;
			this.negativeX = negativeX;
			this.positiveZ = positiveZ;
			this.negativeZ = negativeZ;
			this.stationary = stationary;
		}

		/// <summary>
		/// Gets the offset associated with a cardinal point.
		/// </summary>
		public Vector3 Get(Erelia.Battle.Voxel.CardinalPoint entryPoint)
		{
			// Return the offset for the requested entry point.
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

		/// <summary>
		/// Creates a default cardinal point set at the cell center.
		/// </summary>
		public static CardinalPointSet CreateDefault()
		{
			// Build a set where all points are at the center/top.
			Vector3 origin = new Vector3(0.5f, 1f, 0.5f);
			return new CardinalPointSet(origin, origin, origin, origin, origin);
		}
	}
}
