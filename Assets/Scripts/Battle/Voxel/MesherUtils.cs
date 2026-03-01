using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel.MesherUtils
{
	public static class CardinalPointSetCache
	{
		private readonly struct Key : IEquatable<Key>
		{
			private readonly CardinalPointSet entryPoints;
			private readonly Erelia.Core.VoxelKit.Orientation orientation;
			private readonly Erelia.Core.VoxelKit.FlipOrientation flipOrientation;

			public Key(CardinalPointSet entryPoints, Erelia.Core.VoxelKit.Orientation orientation, Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
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

		private static readonly Dictionary<Key, CardinalPointSet> Collection = new Dictionary<Key, CardinalPointSet>();

		public static bool TryGetValue(
			CardinalPointSet entryPoints,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation,
			out CardinalPointSet output)
		{
			output = null;
			if (entryPoints == null)
			{
				return false;
			}

			var key = new Key(entryPoints, orientation, flipOrientation);
			if (Collection.TryGetValue(key, out CardinalPointSet cached))
			{
				output = cached;
				return true;
			}

			output = TransformCardinalPoints(entryPoints, orientation, flipOrientation);
			Collection[key] = output;
			return true;
		}

		private static CardinalPointSet TransformCardinalPoints(
			CardinalPointSet source,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
		{
			Vector3 posX = Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(source.PositiveX, orientation, flipOrientation);
			Vector3 negX = Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(source.NegativeX, orientation, flipOrientation);
			Vector3 posZ = Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(source.PositiveZ, orientation, flipOrientation);
			Vector3 negZ = Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(source.NegativeZ, orientation, flipOrientation);
			Vector3 stationary = Erelia.Core.VoxelKit.Utils.Geometry.TransformPoint(source.Stationary, orientation, flipOrientation);
			return new CardinalPointSet(posX, negX, posZ, negZ, stationary);
		}
	}
}
