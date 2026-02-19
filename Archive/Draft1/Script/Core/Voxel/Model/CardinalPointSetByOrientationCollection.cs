using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Voxel.Model
{
	public class CardinalPointSetByOrientationCollection
	{
		public struct Key : IEquatable<Key>
		{
			private readonly CardinalPointSet entryPoints;
			private readonly Orientation orientation;
			private readonly FlipOrientation flipOrientation;

			public Key(CardinalPointSet entryPoints, Orientation orientation, FlipOrientation flipOrientation)
			{
				this.entryPoints = entryPoints;
				this.orientation = orientation;
				this.flipOrientation = flipOrientation;
			}

			public bool Equals(Key other)
			{
				return ReferenceEquals(entryPoints, other.entryPoints)
					&& orientation == other.orientation
					&& flipOrientation == other.flipOrientation;
			}

			public override bool Equals(object obj)
			{
				return obj is Key other && Equals(other);
			}

			public override int GetHashCode()
			{
				int hash = entryPoints != null ? entryPoints.GetHashCode() : 0;
				unchecked
				{
					hash = (hash * 397) ^ (int)orientation;
					hash = (hash * 397) ^ (int)flipOrientation;
				}
				return hash;
			}
		}

		private readonly Dictionary<Key, CardinalPointSet> _collection = new Dictionary<Key, CardinalPointSet>();

		public bool TryGetValue(CardinalPointSet entryPoints, Orientation orientation, FlipOrientation flipOrientation, out CardinalPointSet output)
		{
			output = null;
			if (entryPoints == null)
			{
				return false;
			}

			var key = new Key(entryPoints, orientation, flipOrientation);
			if (_collection.TryGetValue(key, out CardinalPointSet cached))
			{
				output = cached;
				return true;
			}

			output = TransformCardinalPoints(entryPoints, orientation, flipOrientation);
			_collection[key] = output;
			return true;
		}

		private static CardinalPointSet TransformCardinalPoints(CardinalPointSet source, Orientation orientation, FlipOrientation flipOrientation)
		{
			Vector3 posX = Core.Utils.Geometry.TransformPoint(source.PositiveX, orientation, flipOrientation);
			Vector3 negX = Core.Utils.Geometry.TransformPoint(source.NegativeX, orientation, flipOrientation);
			Vector3 posZ = Core.Utils.Geometry.TransformPoint(source.PositiveZ, orientation, flipOrientation);
			Vector3 negZ = Core.Utils.Geometry.TransformPoint(source.NegativeZ, orientation, flipOrientation);
			Vector3 stationary = Core.Utils.Geometry.TransformPoint(source.Stationary, orientation, flipOrientation);
			return new CardinalPointSet(posX, negX, posZ, negZ, stationary);
		}
	}
}
