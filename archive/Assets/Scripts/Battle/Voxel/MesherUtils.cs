using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel.MesherUtils
{
	public static class CardinalPointSetByOrientationCache
	{
		private readonly struct Key : IEquatable<Key>
		{
			private readonly CardinalPointSet source;
			private readonly Erelia.Core.Voxel.Orientation orientation;
			public Key(
				CardinalPointSet source,
				Erelia.Core.Voxel.Orientation orientation)
			{
				this.source = source;
				this.orientation = orientation;
			}

			public bool Equals(Key other)
			{
				return ReferenceEquals(source, other.source)
					&& orientation == other.orientation;
			}

			public override bool Equals(object obj)
			{
				return obj is Key other && Equals(other);
			}

			public override int GetHashCode()
			{
				int hash = source != null ? source.GetHashCode() : 0;
				unchecked
				{
					hash = (hash * 397) ^ (int)orientation;
				}
				return hash;
			}
		}

		private static readonly Dictionary<Key, CardinalPointSet> Collection
			= new Dictionary<Key, CardinalPointSet>();

		public static bool TryGetValue(
			CardinalPointSet source,
			Erelia.Core.Voxel.Orientation orientation,
			out CardinalPointSet output)
		{
			output = null;
			if (source == null)
			{
				return false;
			}

			var key = new Key(source, orientation);
			if (Collection.TryGetValue(key, out CardinalPointSet cached))
			{
				output = cached;
				return true;
			}

			output = TransformCardinalPoints(source, orientation);
			Collection[key] = output;
			return true;
		}

		private static CardinalPointSet TransformCardinalPoints(
			CardinalPointSet source,
			Erelia.Core.Voxel.Orientation orientation)
		{
			Vector3 posX = TransformPointForWorldDirection(source, Erelia.Battle.Voxel.CardinalPoint.PositiveX, orientation);
			Vector3 negX = TransformPointForWorldDirection(source, Erelia.Battle.Voxel.CardinalPoint.NegativeX, orientation);
			Vector3 posZ = TransformPointForWorldDirection(source, Erelia.Battle.Voxel.CardinalPoint.PositiveZ, orientation);
			Vector3 negZ = TransformPointForWorldDirection(source, Erelia.Battle.Voxel.CardinalPoint.NegativeZ, orientation);
			Vector3 stationary = Erelia.Core.Voxel.Utils.Geometry.TransformPoint(
				source.Stationary,
				orientation,
				Erelia.Core.Voxel.FlipOrientation.PositiveY);
			return new CardinalPointSet(posX, negX, posZ, negZ, stationary);
		}

		private static Vector3 TransformPointForWorldDirection(
			CardinalPointSet source,
			Erelia.Battle.Voxel.CardinalPoint worldDirection,
			Erelia.Core.Voxel.Orientation orientation)
		{
			Erelia.Battle.Voxel.CardinalPoint localDirection = ResolveLocalDirection(worldDirection, orientation);
			return Erelia.Core.Voxel.Utils.Geometry.TransformPoint(
				source.Get(localDirection),
				orientation,
				Erelia.Core.Voxel.FlipOrientation.PositiveY);
		}

		private static Erelia.Battle.Voxel.CardinalPoint ResolveLocalDirection(
			Erelia.Battle.Voxel.CardinalPoint worldDirection,
			Erelia.Core.Voxel.Orientation orientation)
		{
			switch (orientation)
			{
				case Erelia.Core.Voxel.Orientation.PositiveX:
					return worldDirection;
				case Erelia.Core.Voxel.Orientation.PositiveZ:
					return worldDirection switch
					{
						Erelia.Battle.Voxel.CardinalPoint.PositiveX => Erelia.Battle.Voxel.CardinalPoint.PositiveZ,
						Erelia.Battle.Voxel.CardinalPoint.NegativeX => Erelia.Battle.Voxel.CardinalPoint.NegativeZ,
						Erelia.Battle.Voxel.CardinalPoint.PositiveZ => Erelia.Battle.Voxel.CardinalPoint.NegativeX,
						Erelia.Battle.Voxel.CardinalPoint.NegativeZ => Erelia.Battle.Voxel.CardinalPoint.PositiveX,
						_ => Erelia.Battle.Voxel.CardinalPoint.Stationary
					};
				case Erelia.Core.Voxel.Orientation.NegativeX:
					return worldDirection switch
					{
						Erelia.Battle.Voxel.CardinalPoint.PositiveX => Erelia.Battle.Voxel.CardinalPoint.NegativeX,
						Erelia.Battle.Voxel.CardinalPoint.NegativeX => Erelia.Battle.Voxel.CardinalPoint.PositiveX,
						Erelia.Battle.Voxel.CardinalPoint.PositiveZ => Erelia.Battle.Voxel.CardinalPoint.NegativeZ,
						Erelia.Battle.Voxel.CardinalPoint.NegativeZ => Erelia.Battle.Voxel.CardinalPoint.PositiveZ,
						_ => Erelia.Battle.Voxel.CardinalPoint.Stationary
					};
				case Erelia.Core.Voxel.Orientation.NegativeZ:
					return worldDirection switch
					{
						Erelia.Battle.Voxel.CardinalPoint.PositiveX => Erelia.Battle.Voxel.CardinalPoint.NegativeZ,
						Erelia.Battle.Voxel.CardinalPoint.NegativeX => Erelia.Battle.Voxel.CardinalPoint.PositiveZ,
						Erelia.Battle.Voxel.CardinalPoint.PositiveZ => Erelia.Battle.Voxel.CardinalPoint.PositiveX,
						Erelia.Battle.Voxel.CardinalPoint.NegativeZ => Erelia.Battle.Voxel.CardinalPoint.NegativeX,
						_ => Erelia.Battle.Voxel.CardinalPoint.Stationary
					};
				default:
					return worldDirection;
			}
		}
	}
}

