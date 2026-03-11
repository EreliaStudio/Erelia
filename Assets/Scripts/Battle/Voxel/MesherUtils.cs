using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel.MesherUtils
{
	/// <summary>
	/// Caches transformed cardinal point sets by (source set reference, orientation, flip).
	/// </summary>
	/// <remarks>
	/// This mirrors the role of <see cref="Erelia.Core.VoxelKit.MesherUtils.FaceByOrientationCache"/>,
	/// but for battle voxel cardinal point sets instead of faces.
	/// The key uses reference equality for the source set.
	/// </remarks>
	public static class CardinalPointSetByOrientationCache
	{
		/// <summary>
		/// Cache key for transformed cardinal point sets.
		/// </summary>
		private readonly struct Key : IEquatable<Key>
		{
			/// <summary>
			/// Source cardinal point set.
			/// </summary>
			private readonly CardinalPointSet source;
			/// <summary>
			/// Orientation applied to the point set.
			/// </summary>
			private readonly Erelia.Core.VoxelKit.Orientation orientation;
			/// <summary>
			/// Flip orientation applied to the point set.
			/// </summary>
			private readonly Erelia.Core.VoxelKit.FlipOrientation flipOrientation;

			/// <summary>
			/// Creates a cache key from a point set and orientations.
			/// </summary>
			public Key(
				CardinalPointSet source,
				Erelia.Core.VoxelKit.Orientation orientation,
				Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
			{
				this.source = source;
				this.orientation = orientation;
				this.flipOrientation = flipOrientation;
			}

			/// <summary>
			/// Tests equality against another key.
			/// </summary>
			public bool Equals(Key other)
			{
				return ReferenceEquals(source, other.source)
					&& orientation == other.orientation
					&& flipOrientation == other.flipOrientation;
			}

			/// <summary>
			/// Object equality override.
			/// </summary>
			public override bool Equals(object obj)
			{
				return obj is Key other && Equals(other);
			}

			/// <summary>
			/// Computes a hash code for dictionary usage.
			/// </summary>
			public override int GetHashCode()
			{
				int hash = source != null ? source.GetHashCode() : 0;
				unchecked
				{
					hash = (hash * 397) ^ (int)orientation;
					hash = (hash * 397) ^ (int)flipOrientation;
				}
				return hash;
			}
		}

		private static readonly Dictionary<Key, CardinalPointSet> Collection
			= new Dictionary<Key, CardinalPointSet>();

		/// <summary>
		/// Gets or computes a transformed cardinal point set.
		/// </summary>
		public static bool TryGetValue(
			CardinalPointSet source,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation,
			out CardinalPointSet output)
		{
			output = null;
			if (source == null)
			{
				return false;
			}

			var key = new Key(source, orientation, flipOrientation);
			if (Collection.TryGetValue(key, out CardinalPointSet cached))
			{
				output = cached;
				return true;
			}

			output = TransformCardinalPoints(source, orientation, flipOrientation);
			Collection[key] = output;
			return true;
		}

		/// <summary>
		/// Transforms a cardinal point set by orientation and flip.
		/// </summary>
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
